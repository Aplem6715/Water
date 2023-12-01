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
    public partial struct MoveJob : IJobEntity
    {
        [ReadOnly] public double Dt;

        public void Execute(in Force force, ref Velocity velocity, ref LeapFrog leapFrog, ref LocalTransform transform)
        {
            leapFrog.DelayVelocity += force.Value * Dt;
            transform.Position += new float3(new float2(leapFrog.DelayVelocity * Dt), 0);
            velocity.Value = leapFrog.DelayVelocity + 0.5 * force.Value * Dt;
        }
    }
}