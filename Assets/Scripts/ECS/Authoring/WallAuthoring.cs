using System.Collections;
using System.Collections.Generic;
using SPH.ECS;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SPH.ECS
{

    public class WallAuthoring : MonoBehaviour
    {
        private class WallBaker : Baker<WallAuthoring>
        {
            public override void Bake(WallAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Static());
            }
        }
    }
}
