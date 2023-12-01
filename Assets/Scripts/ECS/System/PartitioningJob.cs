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
        [WriteOnly] public NativeParallelMultiHashMap<int2, Entity>.ParallelWriter Grid;
        [ReadOnly] public double RangeH;

        public void Execute(Entity entity, in LocalTransform transform)
        {
            Grid.Add((int2)((double2)transform.Position.xy / RangeH), entity);
        }
    }
}