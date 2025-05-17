using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Utilities
{
    public static class FloatExtensions
    {
        //https://roundwide.com/equality-comparison-of-floating-point-numbers-in-csharp/
        public static bool EqualsWithTolerance(this float x, float y, float tolerance = 1e-5f)
        {
            var diff = System.Math.Abs(x - y);
            return diff <= tolerance ||
                   diff <= System.Math.Max(System.Math.Abs(x), System.Math.Abs(y)) * tolerance;
        }
    }
}
