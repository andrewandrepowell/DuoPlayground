using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Utilities
{
    public static class Math
    {
        public static int Mod(int x, int m)
        {
            // https://stackoverflow.com/a/51018529
            int r = x % m;
            return r < 0 ? r + m : r;
        }
        public static float AngleDifference(float angle1, float angle2)
        {
            // https://stackoverflow.com/a/28037434
            float diff = (angle2 - angle1 + MathHelper.Pi) % MathHelper.TwoPi - MathHelper.Pi;
            return diff < -MathHelper.Pi ? diff + MathHelper.TwoPi : diff;
        }
        public static Vector2 Vectorize(float radians)
        {
            return new Vector2(
                x: (float)System.Math.Cos(radians), 
                y: (float)System.Math.Sin(radians));
        }
    }
}
