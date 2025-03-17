using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Extended;
using Microsoft.Xna.Framework;

namespace Pow.Utilities
{
    public static class SizeExtensions
    {
        public static Vector2 ToVector2(this Size size)
        {
            return new Vector2(size.Width, size.Height);
        }
    }
}
