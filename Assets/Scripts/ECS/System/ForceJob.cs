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
    public partial struct ForceJob : IJobEntity
    {
        [ReadOnly] public float H;
        [ReadOnly] public double Mass;
        [ReadOnly] public double Viscosity;
        [ReadOnly] public NativeParallelMultiHashMap<int2, Entity>.ReadOnly _grid;
        [ReadOnly] public ComponentLookup<LocalTransform> _transformLookup;
        [ReadOnly] public ComponentLookup<Velocity> _velocityLookup;
        [ReadOnly] public ComponentLookup<Particle> _particleLookup;

        public void Execute(
            in LocalTransform transform,
            in Velocity velocity,
            in Particle p,
            ref Force force)
        {
            double2 add = 0;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int x = (int)(transform.Position.x / H) + dx;
                    int y = (int)(transform.Position.y / H) + dy;
                    if (!_grid.TryGetFirstValue(new int2(x, y), out var pNearEntity, out var iter))
                    {
                        continue;
                    }

                    var pPos = new double2(transform.Position.xy);
                    var pNearPos = new double2(_transformLookup[pNearEntity].Position.xy);

                    double rSqr = math.distancesq(pNearPos, pPos);
                    if (rSqr >= H * H) continue;

                    var v2 = _velocityLookup[pNearEntity];
                    var pNear = _particleLookup[pNearEntity];
                    add += CalcForceAddition(pPos, pNearPos, velocity, v2, p, pNear);

                    while (_grid.TryGetNextValue(out pNearEntity, ref iter))
                    {
                        pNearPos = new double2(_transformLookup[pNearEntity].Position.xy);

                        rSqr = math.distancesq(pNearPos, pPos);
                        if (rSqr >= H * H) continue;

                        v2 = _velocityLookup[pNearEntity];
                        pNear = _particleLookup[pNearEntity];
                        add += CalcForceAddition(pPos, pNearPos, velocity, v2, p, pNear);
                    }
                }
            }

            force.Value += add;
        }

        public double2 CalcForceAddition(double2 pos1, double2 pos2, in Velocity v1, in Velocity v2, in Particle p, in Particle pNear)
        {
            double2 diff = pos1 - pos2;
            double rSqr = math.lengthsq(diff);

            if (rSqr >= H * H) return double2.zero;

            double2 wp = Kernel.Ply6Grad(diff);
            double nearPress = pNear.Pressure / (pNear.Density * pNear.Density);
            double pPress = p.Pressure / (p.Density * p.Density);
            double fp = -Mass * (nearPress + pPress);
            double2 result = wp * fp;

            double r2 = rSqr + 0.01 * H * H;
            double2 dv = v1.Value - v2.Value;
            double fv = Mass * 2 * Viscosity / (pNear.Density * p.Density) * math.dot(diff, wp) / r2;
            result += fv * dv;

            return result;
        }
    }
}