using System.Collections;
using System.Collections.Generic;
using SPH.ECS;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SPH.ECS
{

    public class ParticleAuthoring : MonoBehaviour
    {
        private class ParticleBaker : Baker<ParticleAuthoring>
        {
            public override void Bake(ParticleAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Particle());
                AddComponent(entity, new Velocity());
                AddComponent(entity, new LeapFrog());
                AddComponent(entity, new Force());
            }
        }
    }
}
