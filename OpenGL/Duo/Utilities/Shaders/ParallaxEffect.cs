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
        private readonly EffectParameter _positionEP;
        private readonly EffectParameter _scaleEP;
        private Texture2D _parallaxTexture;
        private Vector2 _position;
        private SizeF _spriteSize;
        private Vector2 _scale;
        public override Effect Effect => _effect;
        public ParallaxEffect()
        {
            _effect = Pow.Globals.Game.Content.Load<Effect>("effects/parallax_0");
            _parallaxTextureEP = _effect.Parameters["ParallaxTexture"];
            _spriteTextureDimensionsEP = _effect.Parameters["SpriteTextureDimensions"];
            _parallaxTextureDimenionsEP = _effect.Parameters["ParallaxTextureDimensions"];
            _positionEP = _effect.Parameters["Position"];
            _scaleEP = _effect.Parameters["Scale"];
        }
        public void Configure(Texture2D parallaxTexture, Vector2 position, SizeF spriteSize, Vector2? scale = null)
        {
            if (!scale.HasValue)
                scale = new(1, 1);
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
            if (_position != position)
            {
                _position = position;
                _positionEP.SetValue(position);
            }
            if (scale.Value != _scale)
            {
                _scale = scale.Value;
                _scaleEP.SetValue(scale.Value);
            }
        }
    }
}
