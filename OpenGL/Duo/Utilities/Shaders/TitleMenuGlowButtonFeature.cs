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
    public class TitleMenuGlowButtonFeature : FeatureManager<TitleMenuGlowButtonEffect>
    {
        private float _glowIntensity;
        private Layers _layer;
        private Color _glowColor;
        public override Layers Layer { get => _layer; set => _layer = value; }
        public Color GlowColor { get => _glowColor; set => _glowColor = value; }
        public float GlowIntensity
        {
            get => _glowIntensity;
            set
            {
                Debug.Assert(value >= 0 && value <= 1.0f);
                _glowIntensity = value;
            }
        }
        protected override void Initialize()
        {
            base.Initialize();
            _glowIntensity = 0;
        }
        public override void UpdateEffect(in Matrix viewProjection)
        {
            base.UpdateEffect(in viewProjection);
            GetEffect().Configure(
                time: (float)Pow.Globals.GameTime.TotalGameTime.TotalSeconds,
                textureSize: new SizeF(
                    width: Parent.Texture.Width, 
                    height: Parent.Texture.Height),
                glowIntensity: _glowIntensity,
                glowColor: _glowColor);
        }
    }
}
