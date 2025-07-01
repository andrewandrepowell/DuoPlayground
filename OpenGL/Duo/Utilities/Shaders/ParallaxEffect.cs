using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pow;
using Pow.Utilities.Shaders;

namespace Duo.Utilities.Shaders
{
    public class ParallaxEffect : BaseEffect
    {
        private readonly Effect _effect;
        private readonly EffectParameter _parallaxTextureEP;
        private readonly EffectParameter _spriteTextureDimensionsEP;
        private readonly EffectParameter _parallaxTextureDimenionsEP;
        private readonly EffectParameter _parallaxRegionEP;
        private readonly EffectParameter _positionEP;
        private Texture2D _parallaxTexture;
        private RectangleF _parallaxRegion;
        private Vector2 _position;
        private SizeF _spriteSize;
        public override Effect Effect => _effect;
        public ParallaxEffect()
        {
            _effect = Pow.Globals.Game.Content.Load<Effect>("effects/parallax_0");
            _parallaxTextureEP = _effect.Parameters["ParallaxTexture"];
            _spriteTextureDimensionsEP = _effect.Parameters["SpriteTextureDimensions"];
            _parallaxTextureDimenionsEP = _effect.Parameters["ParallaxTextureDimensions"];
            _parallaxRegionEP = _effect.Parameters["ParallaxRegion"];
            _positionEP = _effect.Parameters["Position"];
        }
        public void Configure(Texture2D parallaxTexture, RectangleF parallaxRegion, Vector2 position, SizeF spriteSize)
        {
            if (spriteSize != _spriteSize)
            {
                _spriteSize = spriteSize;
                _spriteTextureDimensionsEP.SetValue(spriteSize);
            }
            if (parallaxTexture != _parallaxTexture)
            {
                _parallaxTexture = parallaxTexture;
                _parallaxTextureEP.SetValue(parallaxTexture);
                _parallaxTextureDimenionsEP.SetValue(new Vector2(parallaxTexture.Width, parallaxTexture.Height));
            }
            if (parallaxRegion != _parallaxRegion)
            {
                _parallaxRegion = parallaxRegion;
                _parallaxRegionEP.SetValue(new Vector4(parallaxRegion.X, parallaxRegion.Y, parallaxRegion.Width, parallaxRegion.Height));
            }
            if (_position != position)
            {
                _position = position;
                _positionEP.SetValue(position);
            }
        }
    }
}
