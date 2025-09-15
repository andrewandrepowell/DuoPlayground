using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Utilities.Shaders
{
    using Microsoft.Xna.Framework;
    using Pow.Utilities;
    using Pow.Utilities.Shaders;
    using MonoGame.Extended;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace Duo.Utilities.Shaders
    {
        public class WindedBushFeature : FeatureManager<WindedBushEffect>
        {
            private float _effectScale;
            private float _seed;
            private Layers _layer;
            public override Layers Layer { get => _layer; set => _layer = value; }
            public float EffectScale
            {
                get => _effectScale;
                set
                {
                    Debug.Assert(value >= 0);
                    _effectScale = value;
                }
            }
            public float Seed
            {
                get => _seed;
                set => _seed = value;
            }
            protected override void Initialize()
            {
                base.Initialize();
                _effectScale = 1.0f;
                _seed = 0.0f;
            }
            public override void UpdateEffect(in Matrix viewProjection)
            {
                base.UpdateEffect(in viewProjection);
                GetEffect().Configure(
                    viewProjection: in viewProjection,
                    time: (float)Pow.Globals.GameTime.TotalGameTime.TotalSeconds,
                    textureSize: new SizeF(
                        width: Parent.Texture.Width,
                        height: Parent.Texture.Height),
                    regionSize: Parent.Region.Size,
                    regionOffset: Parent.Region.Location.ToVector2(),
                    scale: _effectScale,
                    seed: _seed);
            }
        }
    }
}
