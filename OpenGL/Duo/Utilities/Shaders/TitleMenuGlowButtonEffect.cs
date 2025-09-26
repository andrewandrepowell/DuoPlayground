using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Pow.Utilities.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Utilities.Shaders
{
    public class TitleMenuGlowButtonEffect : BaseEffect
    {
        private readonly Effect _effect;
        private readonly EffectParameter _timeEP;
        private readonly EffectParameter _spriteTextureSizeEP;
        private readonly EffectParameter _glowIntensitiyEP;
        private float _time;
        private SizeF _textureSize;
        private float _glowIntensity;
        public override Effect Effect => _effect;
        public TitleMenuGlowButtonEffect()
        {
            _effect = Pow.Globals.Game.Content.Load<Effect>("effects/pulse_glow_1");
            _timeEP = _effect.Parameters["Time"];
            _spriteTextureSizeEP = _effect.Parameters["SpriteTextureSize"];
            _glowIntensitiyEP = _effect.Parameters["GlowIntensitiy"];
        }
        public void Configure(
            in float time,
            in SizeF textureSize,
            in float glowIntensity)
        {
            if (_time != time)
            {
                _time = time;
                _timeEP.SetValue(time);
            }
            if (_textureSize != textureSize)
            {
                _textureSize = textureSize;
                _spriteTextureSizeEP.SetValue(textureSize);
            }
            if (_glowIntensity != glowIntensity)
            {
                _glowIntensity = glowIntensity;
                _glowIntensitiyEP.SetValue(glowIntensity);
            }
        }
    }
}
