using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditorInternal;
using UnityEngine;

namespace SPH
{
    public class Grid
    {
        private List<Particle>[] _cells;

        private int Width;
        private int Height;

        public Grid(int width, int height)
        {
            Width = width;
            Height = height;

            _cells = new List<Particle>[Width * Height];
            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i] = new List<Particle>();
            }
        }

        public void Register(int x, int y, Particle p)
        {
            var list = GetParticlesInCell(x, y);
            if (list == null)
            {
                return;
            }

            list.Add(p);
        }

        public List<Particle> GetParticlesInCell(int x, int y)
        {
            var idx = GetIndex(x, y);
            if (idx < 0 || idx >= _cells.Length)
            {
                return null;
            }
            return _cells[idx];
        }

        public int GetIndex(int x, int y)
        {
            if (x < 0 || y < 0)
            {
                return -1;
            }
            return x + Width * y;
        }

        public void Clear()
        {
            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i].Clear();
            }
        }
    }
}
