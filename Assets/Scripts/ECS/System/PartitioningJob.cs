using System;
using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SPH.ECS
{
    [BurstCompile]
    public partial struct PartitioningJob : IJobEntity
    {
        [WriteOnly] public NativeParallelMultiHashMap<int2, Entity>.ParallelWriter _grid;
        [ReadOnly] public float H;

        public void Execute(Entity entity, in LocalTransform transform)
        {
            _grid.Add(new int2(transform.Position.xy / H), entity);
        }
    }
}