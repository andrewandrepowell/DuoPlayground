using Microsoft.Xna.Framework.Graphics;
using Pow.Utilities.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System.Numerics;

namespace Duo.Utilities.Shaders
{
    public class PulseGlowEffect : BaseEffect
    {
        private readonly Effect _effect;
        private readonly EffectParameter _spriteTextureDimensionsEP;
        private readonly EffectParameter _colorEP;
        private readonly EffectParameter _gameTimeSecondsEP;
        private SizeF _spriteSize;
        private Color _color;
        private float _gameTimeSeconds;
        public override Effect Effect => _effect;
        public PulseGlowEffect()
        {
            _effect = Pow.Globals.Game.Content.Load<Effect>("effects/pulse_glow_0");
            _spriteTextureDimensionsEP = _effect.Parameters["SpriteTextureDimensions"];
            _colorEP = _effect.Parameters["Color"];
            _gameTimeSecondsEP = _effect.Parameters["GameTimeSeconds"];
        }
        public void Configure(SizeF spriteSize, Color color)
        {
            if (spriteSize != _spriteSize)
            {
                _spriteSize = spriteSize;
                _spriteTextureDimensionsEP.SetValue(spriteSize);
            }
            if (color != _color)
            {
                _color = color;
                _colorEP.SetValue(color.ToVector4());
            }
            var gameTimeSeconds = (float)Pow.Globals.GameTime.TotalGameTime.TotalSeconds;
            if (gameTimeSeconds != _gameTimeSeconds)
            {
                _gameTimeSeconds = gameTimeSeconds;
                _gameTimeSecondsEP.SetValue(gameTimeSeconds);
            }
        }
    }
}
