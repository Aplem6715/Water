using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace SPH
{

    public class Simulator : MonoBehaviour
    {
        [SerializeField]
        private GameObject _prefab;
        [SerializeField]
        private GameObject _wallPrefab;
        [SerializeField]
        private int2 _numParticles;

        private List<GameObject> _particles = new List<GameObject>();
        private List<GameObject> _walls = new List<GameObject>();

        private SPH _sph;

        // Start is called before the first frame update
        void Awake()
        {
            _sph = new SPH();
            _sph.OnParticleCreated += CreateParticle;
            _sph.OnWallCreated += CreateWall;
            _sph.Init(_numParticles);
        }

        private void CreateParticle(Particle p)
        {
            var particle = Instantiate(_prefab);
            var component = particle.GetComponent<MonoParticle>();
            component.Link(p);
        }

        private void CreateWall(Particle p)
        {
            var particle = Instantiate(_wallPrefab);
            var component = particle.GetComponent<MonoParticle>();
            component.Link(p);
        }

        private void FixedUpdate()
        {
            _sph.Step(0.001);
        }
    }
}
