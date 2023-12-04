using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace SPH.Compute
{
    public class ComputeShaderSPH : MonoBehaviour
    {
        [SerializeField] private ComputeShader _shader;
        [SerializeField] private RenderTexture _liquidTex;
        [SerializeField] private int2 NumParticles;
        [SerializeField] private int2 FieldSize;
        [SerializeField] int WallThickness = 4;
        [SerializeField] int ParticleRenderSize = 1;

        private int _numParticles;
        private int _numWalls;

        private int _kernelSpawn;
        private int _kernelPressureDensity;
        private int _kernelForceMove;
        private int _kernelRender;
        private int _kernelClear;

        private GraphicsBuffer _particleBuffer;

        private struct Particle
        {
            float2 pos;
            float2 v;
            float2 v2;
            float density;
            float pressure;
            int isStatic;
            int isActive;
        }

        private void Awake()
        {
            _kernelSpawn = _shader.FindKernel("SpawnParticle");
            _kernelPressureDensity = _shader.FindKernel("UpdatePressure");
            _kernelForceMove = _shader.FindKernel("UpdateParticle");
            _kernelRender = _shader.FindKernel("RenderParticle");
            _kernelClear = _shader.FindKernel("ClearLiquidTex");

            SetupBuffers();
        }

        private void Start()
        {
            _shader.Dispatch(_kernelSpawn, 1, 1, 1);
        }

        private void SetupBuffers()
        {
            _numParticles = NumParticles.x * NumParticles.y;
            _numWalls = WallThickness * 2 * (FieldSize.x + WallThickness * 2) + WallThickness * 2 * FieldSize.y;
            _particleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _numParticles + _numWalls, Marshal.SizeOf<Particle>());

            _shader.SetBuffer(_kernelSpawn, "_Particles", _particleBuffer);
            _shader.SetBuffer(_kernelPressureDensity, "_Particles", _particleBuffer);
            _shader.SetBuffer(_kernelForceMove, "_Particles", _particleBuffer);
            _shader.SetBuffer(_kernelRender, "_Particles", _particleBuffer);
            _shader.SetBuffer(_kernelClear, "_Particles", _particleBuffer);

            _shader.SetTexture(_kernelRender, "_LiquidTex", _liquidTex);
            _shader.SetTexture(_kernelClear, "_LiquidTex", _liquidTex);

            _shader.SetInt("_numParticles", _numParticles);
            _shader.SetInt("_numParticleX", NumParticles.x);
            _shader.SetInt("_numParticleY", NumParticles.y);
            _shader.SetInt("_fieldWidth", FieldSize.x);
            _shader.SetInt("_fieldHeight", FieldSize.y);
            _shader.SetInt("_numParticleAndWall", _numParticles + _numWalls);
            _shader.SetInt("_WallThickness", WallThickness);
            _shader.SetInt("_ParticleRenderSize", ParticleRenderSize);
        }

        private void Update()
        {
            // マウスの左ボタンがクリックされたかをチェック
            if (Input.GetMouseButtonDown(0))
            {
                _shader.Dispatch(_kernelSpawn, 1, 1, 1);
                Debug.Log("Spawn");
            }
        }

        private void FixedUpdate()
        {
            _shader.SetFloat("_dt", 0.0005f);

            _shader.Dispatch(_kernelClear, _liquidTex.width / 8, _liquidTex.height / 8, 1);

            int numThreadGroup = (int)math.ceil((_numParticles + _numWalls) / 64.0);
            _shader.Dispatch(_kernelPressureDensity, numThreadGroup, 1, 1);
            _shader.Dispatch(_kernelForceMove, numThreadGroup, 1, 1);
            _shader.Dispatch(_kernelRender, numThreadGroup, 1, 1);
        }

        private void OnDestroy()
        {
            _particleBuffer.Dispose();
        }
    }
}