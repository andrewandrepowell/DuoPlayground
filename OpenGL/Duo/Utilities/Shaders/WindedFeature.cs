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
    public class WindedFeature : FeatureManager<WindedEffect>
    {
        private float _glowIntensity;
        private float _effectScale;
        private Layers _layer;
        public override Layers Layer { get => _layer; set => _layer = value; }
        public float GlowIntensity
        {
            get => _glowIntensity;
            set
            {
                Debug.Assert(value >= 0 && value <= 1.0f);
                _glowIntensity = value;
            }
        }
        public float EffectScale
        {
            get => _effectScale;
            set
            {
                Debug.Assert(value >= 0);
                _effectScale = value;
            }
        }
        protected override void Initialize()
        {
            base.Initialize();
            _glowIntensity = 0;
            _effectScale = 1.0f;
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
                glowIntensity: _glowIntensity,
                scale: _effectScale);
        }
    }
}
