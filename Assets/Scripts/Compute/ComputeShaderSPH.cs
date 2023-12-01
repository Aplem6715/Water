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
            float2 accel;
            float density;
            float pressure;
        }

        private void Start()
        {
            _kernelSpawn = _shader.FindKernel("SpawnParticle");
            _kernelPressureDensity = _shader.FindKernel("UpdatePressure");
            _kernelForceMove = _shader.FindKernel("UpdateParticle");
            _kernelRender = _shader.FindKernel("RenderParticle");
            _kernelClear = _shader.FindKernel("ClearLiquidTex");

            SetupBuffers();
        }

        private void SetupBuffers()
        {
            _particleBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, NumParticles.x * NumParticles.y, Marshal.SizeOf<Particle>());

            _shader.SetBuffer(_kernelSpawn, "_Particles", _particleBuffer);
            _shader.SetBuffer(_kernelPressureDensity, "_Particles", _particleBuffer);
            _shader.SetBuffer(_kernelForceMove, "_Particles", _particleBuffer);
            _shader.SetBuffer(_kernelRender, "_Particles", _particleBuffer);
            _shader.SetBuffer(_kernelClear, "_Particles", _particleBuffer);

            _shader.SetTexture(_kernelRender, "_LiquidTex", _liquidTex);
            _shader.SetTexture(_kernelClear, "_LiquidTex", _liquidTex);

            _shader.SetInt("_numParticles", NumParticles.x * NumParticles.y);
            _shader.SetInt("_numParticleX", NumParticles.x);
            _shader.SetInt("_numParticleY", NumParticles.y);
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
            _shader.SetFloat("_dt", Time.deltaTime);

            _shader.Dispatch(_kernelPressureDensity, (int)math.ceil(NumParticles.x * NumParticles.y / 64.0), 1, 1);
            _shader.Dispatch(_kernelForceMove, (int)math.ceil(NumParticles.x * NumParticles.y / 64.0), 1, 1);
            _shader.Dispatch(_kernelRender, (int)math.ceil(NumParticles.x * NumParticles.y / 64.0), 1, 1);
            _shader.Dispatch(_kernelClear, _liquidTex.width / 8, _liquidTex.height / 8, 1);
        }
    }
}