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
    [WithNone(typeof(Static))]
    public partial struct ForceJob : IJobEntity
    {
        [ReadOnly] public double RangeH;
        [ReadOnly] public double RangeSqrH;
        [ReadOnly] public double Poly6Alpha; // = 4.0 / (math.PI * math.pow(h, 8));
        [ReadOnly] public double Mass;
        [ReadOnly] public double Viscosity;
        [ReadOnly] public double2 Gravity;
        [ReadOnly] public NativeParallelMultiHashMap<int2, Entity>.ReadOnly _grid;
        [ReadOnly] public ComponentLookup<LocalTransform> _transformLookup;
        [ReadOnly] public ComponentLookup<Velocity> _velocityLookup;
        [ReadOnly] public ComponentLookup<Particle> _particleLookup;

        public void Execute(
            Entity selfEntity,
            in LocalTransform transform,
            in Velocity velocity,
            in Particle p,
            ref Force force)
        {
            double2 add = 0;
            Velocity v2;
            Particle pNear;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int x = (int)(transform.Position.x / RangeH) + dx;
                    int y = (int)(transform.Position.y / RangeH) + dy;
                    if (!_grid.TryGetFirstValue(new int2(x, y), out var pNearEntity, out var iter))
                    {
                        continue;
                    }

                    double2 pNearPos;
                    double rSqr;
                    var pPos = (double2)transform.Position.xy;

                    if (pNearEntity != selfEntity)
                    {
                        pNearPos = (double2)_transformLookup[pNearEntity].Position.xy;
                        rSqr = math.distancesq(pNearPos, pPos);
                        if (rSqr < RangeSqrH)
                        {
                            v2 = _velocityLookup[pNearEntity];
                            pNear = _particleLookup[pNearEntity];
                            add += CalcForceAddition(pPos, pNearPos, velocity, v2, p, pNear);
                        }
                    }

                    while (_grid.TryGetNextValue(out pNearEntity, ref iter))
                    {
                        if (pNearEntity == selfEntity)
                        {
                            continue;
                        }

                        pNearPos = (double2)_transformLookup[pNearEntity].Position.xy;
                        rSqr = math.distancesq(pNearPos, pPos);
                        if (rSqr >= RangeSqrH) continue;

                        v2 = _velocityLookup[pNearEntity];
                        pNear = _particleLookup[pNearEntity];
                        add += CalcForceAddition(pPos, pNearPos, velocity, v2, p, pNear);
                    }
                }
            }

            add += Gravity;

            force.Value = add;
        }

        public double2 CalcForceAddition(double2 pos1, double2 pos2, in Velocity v1, in Velocity v2, in Particle p, in Particle pNear)
        {
            double2 diff = pos1 - pos2;
            double rSqr = math.lengthsq(diff);

            if (rSqr >= RangeSqrH) return double2.zero;

            double2 wp = Ply6Grad(diff, rSqr);
            double nearPress = pNear.Pressure / (pNear.Density * pNear.Density);
            double pPress = p.Pressure / (p.Density * p.Density);
            double fp = -Mass * (nearPress + pPress);
            double2 result = wp * fp;

            double r2 = rSqr + 0.01 * RangeSqrH;
            double2 dv = v1.Value - v2.Value;
            double fv = Mass * 2 * Viscosity / (pNear.Density * p.Density) * math.dot(diff, wp) / r2;
            result += fv * dv;

            return result;
        }

        public double2 Ply6Grad(double2 diff, double rSqr)
        {
            double diffSqr = RangeSqrH - rSqr;
            double c = -6 * Poly6Alpha * diffSqr * diffSqr;
            return c * diff;
        }
    }
}