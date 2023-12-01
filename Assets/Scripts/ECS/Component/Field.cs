using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SPH.ECS
{
    [BurstCompile]
    public partial struct Field: IComponentData
    {
        public int2 Size;
        public float2 PositionMax;
    }
}