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
    public partial class SpawnSystem : SystemBase
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Spawner>();
        }

        protected override void OnUpdate()
        {
            Entities
                .WithStructuralChanges()
                .ForEach((Entity entity, in Spawner spawner) =>
                {
                    var wallThickness = spawner.WallThickness;
                    int width = spawner.FieldSize.x;
                    int height = spawner.FieldSize.y;
                    float particleSize = (float)SPHSystem.ForceRangeH;

                    EntityManager.CreateSingleton(new Field()
                    {
                        Size = spawner.FieldSize,
                        PositionMax = (float2)spawner.FieldSize * (float)SPHSystem.ForceRangeH
                    });

                    // 通常パーティクル
                    for (int x = 0; x < spawner.NumParticles.x; x++)
                    {
                        for (int y = 0; y < spawner.NumParticles.y; y++)
                        {
                            var e = EntityManager.Instantiate(spawner.ParticlePrefab);
                            var transform = EntityManager.GetComponentData<LocalTransform>(e);
                            transform.Position = new float3((x + wallThickness) * particleSize, (y + wallThickness) * particleSize, 0);
                            EntityManager.SetComponentData(e, transform);
                        }
                    }


                    particleSize = (float)SPHSystem.ParticleSize;


                    // 左右の壁
                    for (int x = 0; x < wallThickness; x++)
                    {
                        for (int y = 0; y < height + wallThickness; y++)
                        {
                            var e = EntityManager.Instantiate(spawner.WallPrefab);
                            var transform = EntityManager.GetComponentData<LocalTransform>(e);
                            transform.Position = new float3(x * particleSize, y * particleSize, 0);
                            EntityManager.SetComponentData(e, transform);

                            e = EntityManager.Instantiate(spawner.WallPrefab);
                            transform = EntityManager.GetComponentData<LocalTransform>(e);
                            transform.Position = new float3((x + width) * particleSize, y * particleSize, 0);
                            EntityManager.SetComponentData(e, transform);
                        }
                    }

                    // 上下の壁
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < wallThickness; y++)
                        {
                            var e = EntityManager.Instantiate(spawner.WallPrefab);
                            var transform = EntityManager.GetComponentData<LocalTransform>(e);
                            transform.Position = new float3(x * particleSize, y * particleSize, 0);
                            EntityManager.SetComponentData(e, transform);

                            e = EntityManager.Instantiate(spawner.WallPrefab);
                            transform = EntityManager.GetComponentData<LocalTransform>(e);
                            transform.Position = new float3(x * particleSize, (y + height) * particleSize, 0);
                            EntityManager.SetComponentData(e, transform);
                        }
                    }

                    EntityManager.RemoveComponent<Spawner>(entity);
                }).Run();
        }

    }

}