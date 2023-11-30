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
    public partial struct DensityPressureJob : IJobEntity
    {
        [ReadOnly] public float H;
        [ReadOnly] public double Poly6Alpha;
        [ReadOnly] public double Mass;
        [ReadOnly] public double Stiffness;
        [ReadOnly] public double Density0;
        [ReadOnly] public NativeParallelMultiHashMap<int2, Entity>.ReadOnly _grid;
        [ReadOnly] public ComponentLookup<LocalTransform> _transformLookup;

        public void Execute(in LocalTransform transform, ref Particle p)
        {
            double density = 0;

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

                    var d = Kernel.Poly6(rSqr) * Mass;
                    density += d;

                    while (_grid.TryGetNextValue(out pNearEntity, ref iter))
                    {
                        pNearPos = new double2(_transformLookup[pNearEntity].Position.xy);
                        rSqr = math.distancesq(pNearPos, pPos);
                        if (rSqr >= H * H) continue;

                        d = Kernel.Poly6(rSqr) * Mass;
                        density += d;
                    }
                }
            }

            p.Density = density;
            p.Pressure = math.max(Stiffness * (density - Density0), 0);
        }
    }
}