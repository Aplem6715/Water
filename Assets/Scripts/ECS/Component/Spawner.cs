using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SPH.ECS
{
    public partial struct Spawner: IComponentData
    {
        public Entity ParticlePrefab;
        public Entity WallPrefab;
        public int2 NumParticles;
        public int2 FieldSize;
        public int WallThickness;
    }
}