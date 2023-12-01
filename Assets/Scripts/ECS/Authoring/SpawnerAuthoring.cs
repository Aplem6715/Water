using System.Collections;
using System.Collections.Generic;
using SPH.ECS;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SPH.ECS
{

    public class SpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private int2 _numParticles;
        [SerializeField] private int2 _fieldSize;
        [SerializeField] private int _wallThickness;

        private class SpawnerBaker : Baker<SpawnerAuthoring>
        {
            public override void Bake(SpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new Spawner()
                {
                    ParticlePrefab = GetEntity(authoring._prefab, TransformUsageFlags.Dynamic),
                    WallPrefab = GetEntity(authoring._wallPrefab, TransformUsageFlags.Renderable),
                    NumParticles = authoring._numParticles,
                    FieldSize = authoring._fieldSize,
                    WallThickness = authoring._wallThickness
                });
            }
        }
    }
}
