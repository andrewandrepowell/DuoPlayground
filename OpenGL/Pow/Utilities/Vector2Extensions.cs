using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Pow.Utilities
{
    public static class Vector2Extensions
    {
        public static Vector2 Average(this IEnumerable<Vector2> vectors)
        {
            if (vectors == null) 
                throw new ArgumentNullException(nameof(vectors));
            var result = new Vector2();
            var total = 0;
            foreach (var v in vectors)
            {
                result.X += v.X;
                result.Y += v.Y;
                total++;
            }
            if (total == 0)
                return result;
            return result / total;
        }
    }
}
