using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace SPH
{
    public class Kernel
    {
        private static double _h;
        private static double _hSqr;
        private static double _alpha;

        public static void Setup(double h)
        {
            _h = h;
            _hSqr = h * h;
            _alpha = 4.0 / (math.PI * math.pow(h, 8));
        }

        public static double Poly6(double rSqr)
        {
            if (rSqr < _hSqr)
            {
                double sqrDiff = _hSqr - rSqr;
                return _alpha * (sqrDiff * sqrDiff * sqrDiff);
            }
            return 0;
        }

        public static double2 Ply6Grad(double2 diff)
        {
            double rSqr = math.lengthsq(diff);
            if (rSqr < _hSqr)
            {
                double diffSqr = _hSqr - rSqr;
                double c = -6 * _alpha * diffSqr * diffSqr;
                return c * diff;
            }
            else
            {
                return double2.zero;
            }
        }
    }
}
