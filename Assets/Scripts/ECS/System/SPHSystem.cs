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
        public const double _h = ParticleSize * 1.5;
        public const double Stiffness = 100.0;
        public const double Density0 = 1000;
        public const double Viscosity = 1.0;
        public const double Mass = ParticleSize * ParticleSize * Density0;
        public const int WallThickness = 4;
        public static readonly double2 Gravity = new double2(0, -9.8);

    }

}