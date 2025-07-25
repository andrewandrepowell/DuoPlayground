using Pow.Utilities;
using Pow.Utilities.Shaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Utilities.Shaders
{
    public class PulseGlowFeature : FeatureManager<PulseGlowEffect>
    {
        private Layers _layer;
        private Color _color;
        public override Layers Layer { get => _layer; set => _layer = value; }
        public Color Color { get => _color; set => _color = value; }
        public override void UpdateEffect()
        {
            base.UpdateEffect();
            GetEffect().Configure(
                color: _color,
                spriteSize: new(
                    width: Parent.Texture.Width,
                    height: Parent.Texture.Height));
        }
    }
}
