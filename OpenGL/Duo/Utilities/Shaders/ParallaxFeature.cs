using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Pow.Utilities;
using Pow.Utilities.Shaders;

namespace Duo.Utilities.Shaders
{
    public class ParallaxFeature : FeatureManager<ParallaxEffect>
    {
        private bool _initialized = false;
        private Layers _layer;
        private IParent _parallaxParent;
        private Vector2 _parallaxPosition;
        public override Layers Layer { get => _layer; set => _layer = value; }
        public Vector2 ParallaxPosition { get => _parallaxPosition; set => _parallaxPosition = value; }
        public void Initialize(IParent parallaxParent)
        {
            Debug.Assert(!_initialized);
            _parallaxParent = parallaxParent;
            _parallaxPosition = Vector2.Zero;
            _initialized = true;
        }
        public override void UpdateEffect(in Matrix viewProjection)
        {
            base.UpdateEffect(in viewProjection);
            Debug.Assert(_initialized);
            GetEffect().Configure(
                parallaxTexture: _parallaxParent.Texture,
                parallaxRegion: _parallaxParent.Region,
                position: _parallaxPosition,
                spriteSize: new(
                    width: Parent.Texture.Width, 
                    height: Parent.Texture.Height));
        }
        protected override void Cleanup()
        {
            Debug.Assert(_initialized);
            _initialized = false;
        }
    }
}
