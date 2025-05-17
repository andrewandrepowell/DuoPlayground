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
    }
}
