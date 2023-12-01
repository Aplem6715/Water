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
    public partial struct DropoutSystem : ISystem
    {
        private EntityQuery _withoutWallsQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Particle>();

            _withoutWallsQuery = new EntityQueryBuilder(Allocator.Persistent)
                .WithNone<Static>()
                .Build(ref state);
        }

        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

            var handle = new DropoutJob()
            {
                Ecb = ecb.AsParallelWriter(),
                Field = SystemAPI.GetSingleton<Field>()
            }.ScheduleParallel(state.Dependency);

            handle.Complete();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        [WithNone(typeof(Static))]
        private partial struct DropoutJob : IJobEntity
        {
            [WriteOnly] public EntityCommandBuffer.ParallelWriter Ecb;
            [ReadOnly] public Field Field;

            public void Execute(Entity self, [ChunkIndexInQuery] int sortKey, in LocalTransform transform)
            {
                if (
                    transform.Position.x > Field.PositionMax.x
                    || transform.Position.x < 0
                    || transform.Position.y > Field.PositionMax.y
                    || transform.Position.y < 0
                    )
                {
                    Ecb.DestroyEntity(sortKey, self);
                }
            }
        }

    }

}