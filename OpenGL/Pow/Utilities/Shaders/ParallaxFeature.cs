using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Pow.Utilities.Shaders
{
    public class ParallaxFeature : FeatureManager<ParallaxEffect>
    {
        private bool _initialized = false;
        private Layers _layer;
        private IParent _parallaxParent;
        public override Layers Layer => _layer;
        public Vector2 ParallaxPosition;
        public void Initialize(Layers layer, IParent parallaxParent)
        {
            Debug.Assert(!_initialized);
            _layer = layer;
            _parallaxParent = parallaxParent;
            _initialized = false;
        }
        public override void UpdateEffect()
        {
            base.UpdateEffect();
            Debug.Assert(_initialized);
            GetEffect().Configure(
                parallaxTexture: _parallaxParent.Texture,
                position: ParallaxPosition,
                spriteSize: new(width: Parent.Texture.Width, height: Parent.Texture.Height));
        }
        public override void Return()
        {
            Debug.Assert(_initialized);
            base.Return();
            _initialized = false;
        }
    }
}
