using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace SPH
{
    public class Particle
    {
        public double2 Pos;
        public double2 Vel = 0;
        public double2 Vel2 = 0;
        public double2 Force = 0;
        public double Pressure = 0;
        public double Density = 0;
        public bool IsActive = true;
        public bool IsStatic = false;

        public Particle(double x, double y, bool isStatic=false)
        {
            Pos = new double2(x, y);
            IsStatic = isStatic;
        }
    }
}
