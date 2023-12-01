using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace SPH.ECS
{
    [BurstCompile]
    public partial struct SPHSystem : ISystem
    {
        public const double DeltaTime = 0.001;
        public const double ParticleSize = 0.01;
        public const double ForceRangeH = ParticleSize * 1.5;
        public const double SqrRangeH = ForceRangeH * ForceRangeH;
        // 4 / (PI * h^8)
        public const double Poly6Alpha = 4.0 / (math.PI * SqrRangeH * SqrRangeH * SqrRangeH * SqrRangeH);
        public const double Stiffness = 100.0;
        public const double Density0 = 1000;
        public const double Viscosity = 1.0;
        public const double Mass = ParticleSize * ParticleSize * Density0;
        public const int WallThickness = 4;
        public static readonly double2 Gravity = new double2(0, -9.8);

        private NativeParallelMultiHashMap<int2, Entity> _gridMap;

        private ComponentLookup<LocalTransform> _transformLookup;
        public ComponentLookup<Velocity> _velocityLookup;
        public ComponentLookup<Particle> _particleLookup;

        private EntityQuery _withoutWallsQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Particle>();

            _withoutWallsQuery = new EntityQueryBuilder(Allocator.Persistent)
                .WithAllRW<Particle>()
                .WithAllRW<Velocity>()
                .WithAllRW<LeapFrog>()
                .WithAllRW<LocalTransform>()
                .WithAllRW<Force>()
                .WithNone<Static>()
                .Build(ref state);

            _gridMap = new NativeParallelMultiHashMap<int2, Entity>(256*256, Allocator.Persistent);
            _transformLookup = state.GetComponentLookup<LocalTransform>();
            _velocityLookup = state.GetComponentLookup<Velocity>();
            _particleLookup = state.GetComponentLookup<Particle>();
        }

        public void OnUpdate(ref SystemState state)
        {
            _transformLookup.Update(ref state);
            _velocityLookup.Update(ref state);
            _particleLookup.Update(ref state);
            _gridMap.Clear();

            var handle = new PartitioningJob()
            {
                Grid = _gridMap.AsParallelWriter(),
                RangeH = ForceRangeH
            }.ScheduleParallel(state.Dependency);

            handle = new DensityPressureJob()
            {
                RangeH = ForceRangeH,
                RangeSqrH = SqrRangeH,
                Poly6Alpha = Poly6Alpha,
                Mass = Mass,
                Stiffness = Stiffness,
                Density0 = Density0,
                _grid = _gridMap.AsReadOnly(),
                _transformLookup = _transformLookup
            }.ScheduleParallel(handle);

            handle = new ForceJob()
            {
                RangeH = ForceRangeH,
                RangeSqrH = SqrRangeH,
                Poly6Alpha = Poly6Alpha,
                Mass = Mass,
                Viscosity = Viscosity,
                Gravity = Gravity,
                _grid = _gridMap.AsReadOnly(),
                _transformLookup = _transformLookup,
                _velocityLookup = _velocityLookup,
                _particleLookup = _particleLookup
            }.ScheduleParallel(_withoutWallsQuery, handle);

            handle = new MoveJob()
            {
                Dt = DeltaTime
            }.ScheduleParallel(_withoutWallsQuery, handle);

            state.Dependency = handle;
        }

        public void OnDestroy(ref SystemState state)
        {
            _gridMap.Dispose();
        }

    }

}