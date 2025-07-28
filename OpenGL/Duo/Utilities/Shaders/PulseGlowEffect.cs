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
        private readonly EffectParameter _periodEP;
        private readonly EffectParameter _timeEP;
        private SizeF _spriteSize;
        private Color _color;
        private float _period;
        private float _time;
        public override Effect Effect => _effect;
        public PulseGlowEffect()
        {
            _effect = Pow.Globals.Game.Content.Load<Effect>("effects/pulse_glow_0");
            _spriteTextureDimensionsEP = _effect.Parameters["SpriteTextureDimensions"];
            _periodEP = _effect.Parameters["Period"];
            _colorEP = _effect.Parameters["Color"];
            _timeEP = _effect.Parameters["Time"];
        }
        public void Configure(SizeF spriteSize, Color color, float time, float period)
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
            if (_period != period)
            {
                _period = period;
                _periodEP.SetValue(period);
            }
            if (_time != time)
            {
                _time = time;
                _timeEP.SetValue(time);
            }
        }
    }
}
