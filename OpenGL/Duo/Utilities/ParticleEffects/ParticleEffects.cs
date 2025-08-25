using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Duo.Utilities
{
    internal static partial class ParticleEffects
    {
        class ExtendedLineProfile : LineProfile
        {
            private Vector2 _direction;
            private float _angle;
            private float _cone;
            public Vector2 Direction 
            { 
                get => _direction; 
                set
                {
                    if (_direction == value) return;
                    _direction = value;
                    _angle = (float)Math.Atan2(_direction.Y, _direction.X);
                }
            }
            public float Cone
            {
                get => _cone;
                set
                {
                    Debug.Assert(value >= 0);
                    _cone = value;
                }
            }
            public override void GetOffsetAndHeading(out Vector2 offset, out Vector2 heading)
            {
                Vector2 vector = Axis * Random.NextSingle(Length * -0.5f, Length * 0.5f);
                offset = new Vector2(vector.X, vector.Y);
                var angle = MathHelper.WrapAngle(_angle + Random.NextSingle() * _cone);
                heading = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            }
        }
        private static Texture2DRegion[] CreateTexture2DRegions(Texture2D texture, Size regionSize)
        {
            var xRegions = texture.Width / regionSize.Width;
            var yRegions = texture.Height / regionSize.Height;
            var totalRegions = xRegions * yRegions;
            var textureRegions = new Texture2DRegion[totalRegions];
            for (var y = 0; y < yRegions; y++)
            {
                for (var x = 0; x < xRegions; x++)
                {
                    textureRegions[x + y * xRegions] = new Texture2DRegion(texture, new Rectangle(
                        x * regionSize.Width,
                        y * regionSize.Height,
                        regionSize.Width,
                        regionSize.Height));
                }
            }
            return textureRegions;
        }
    }
}
