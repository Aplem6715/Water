using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditorInternal;
using UnityEngine;

namespace SPH
{
    public class SPH
    {
        public IReadOnlyList<Particle> Particles => _particles;
        public event Action<Particle> OnParticleCreated;
        public event Action<Particle> OnWallCreated;

        private List<Particle> _particles;
        private Grid _grid;

        private int2 _numParticle2;
        private int _fieldWidth;
        private int _fieldHeight;

        public const double ParticleSize = 0.01;
        public const double _h = ParticleSize * 1.5;
        public const double Stiffness = 100.0;
        public const double Density0 = 1000;
        public const double Viscosity = 1.0;
        public const double Mass = ParticleSize * ParticleSize * Density0;
        public const int WallThickness = 4;
        public static readonly double2 Gravity = new double2(0, -9.8);

        public void Init(int2 numParticles)
        {
            _numParticle2 = numParticles;
            _fieldWidth = numParticles.x * 3;
            _fieldHeight = (int)(numParticles.y * 2);
            _grid = new Grid(_fieldWidth, _fieldHeight);
            Kernel.Setup(_h);
            Reset();
        }

        public void Reset()
        {
            _particles = new List<Particle>();
            for (int x = 0; x < _numParticle2.x; x++)
            {
                for (int y = 0; y < _numParticle2.y; y++)
                {
                    var p = new Particle((x + WallThickness) * ParticleSize, (y + WallThickness) * ParticleSize);
                    _particles.Add(p);
                    OnParticleCreated?.Invoke(p);
                }
            }

            // 左右の壁
            for (int x = 0; x < WallThickness; x++)
            {
                for (int y = 0; y < _fieldHeight + WallThickness; y++)
                {
                    var p = new Particle(x * ParticleSize, y * ParticleSize, true);
                    _particles.Add(p);
                    OnWallCreated?.Invoke(p);

                    p = new Particle((x + _fieldWidth) * ParticleSize, y * ParticleSize, true);
                    _particles.Add(p);
                    OnWallCreated?.Invoke(p);
                }
            }

            // 上下の壁
            for (int x = 0; x < _fieldWidth; x++)
            {
                for (int y = 0; y < WallThickness; y++)
                {
                    var p = new Particle(x * ParticleSize, y * ParticleSize, true);
                    _particles.Add(p);
                    OnWallCreated?.Invoke(p);

                    p = new Particle(x * ParticleSize, (y + _fieldHeight) * ParticleSize, true);
                    _particles.Add(p);
                    OnWallCreated?.Invoke(p);
                }
            }
        }

        public void Step(double dt)
        {
            UpdatePartition();
            CalcDensityAndPressure();
            CalcForce();
            UpdatePos(dt);
        }

        private void UpdatePartition()
        {
            _grid.Clear();
            foreach (Particle p in _particles)
            {
                _grid.Register((int)(p.Pos.x / _h), (int)(p.Pos.y / _h), p);
            }
        }

        private void CalcDensityAndPressure()
        {
            foreach (Particle p in _particles)
            {
                if (!p.IsActive) continue;

                double density = 0;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int x = (int)(p.Pos.x / _h) + dx;
                        int y = (int)(p.Pos.y / _h) + dy;
                        var particlesInCell = _grid.GetParticlesInCell(x, y);
                        if (particlesInCell == null) continue;
                        foreach (var pNear in particlesInCell)
                        {
                            if (!pNear.IsActive) continue;

                            double rSqr = math.distancesq(pNear.Pos, p.Pos);
                            if (rSqr >= _h * _h) continue;

                            var d = Kernel.Poly6(rSqr) * Mass;
                            density += d;
                        }
                    }
                }
                p.Density = density;
                p.Pressure = math.max(Stiffness * (density - Density0), 0);
            }
        }

        private void CalcForce()
        {
            foreach (Particle p in _particles)
            {
                if (!p.IsActive || p.IsStatic) continue;

                double2 force = 0;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int x = (int)(p.Pos.x / _h) + dx;
                        int y = (int)(p.Pos.y / _h) + dy;
                        var particlesInCell = _grid.GetParticlesInCell(x, y);
                        if (particlesInCell == null) continue;
                        foreach (var pNear in particlesInCell)
                        {

                            if (!pNear.IsActive || ReferenceEquals(pNear, p)) continue;

                            double2 diff = p.Pos - pNear.Pos;
                            double rSqr = math.lengthsq(diff);

                            if (rSqr >= _h * _h) continue;

                            double2 wp = Kernel.Ply6Grad(diff);
                            double nearPress = pNear.Pressure / (pNear.Density * pNear.Density);
                            double pPress = p.Pressure / (p.Density * p.Density);
                            double fp = -Mass * (nearPress + pPress);
                            force += wp * fp;

                            double r2 = rSqr + 0.01 * _h * _h;
                            double2 dv = p.Vel - pNear.Vel;
                            double fv = Mass * 2 * Viscosity / (pNear.Density * p.Density) * math.dot(diff, wp) / r2;
                            force += fv * dv;
                        }
                    }
                }

                force += Gravity;

                p.Force = force;
            }
        }

        private void UpdatePos(double dt)
        {
            foreach (Particle p in _particles)
            {
                if (!p.IsActive | p.IsStatic) continue;

                p.Vel2 += p.Force * dt;
                p.Pos += p.Vel2 * dt;
                p.Vel = p.Vel2 + 0.5 * p.Force * dt;
            }
        }

    }
}
