using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pow.Utilities.Shaders;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Utilities.Shaders
{
    public class WindedEffect : BaseEffect
    {
        private readonly Effect _effect;
        private readonly EffectParameter _viewProjectionEP;
        private readonly EffectParameter _timeEP;
        private readonly EffectParameter _spriteTextureSizeEP;
        private readonly EffectParameter _spriteTextureRegionSizeEP;
        private readonly EffectParameter _spriteTextureRegionOffsetEP;
        private readonly EffectParameter _glowIntensitiyEP;
        private Matrix _viewProjection;
        private float _time;
        private SizeF _textureSize;
        private SizeF _regionSize;
        private Vector2 _regionOffset;
        private float _glowIntensity;
        public override Effect Effect => _effect;
        public WindedEffect()
        {
            _effect = Pow.Globals.Game.Content.Load<Effect>("effects/winded_0");
            _viewProjectionEP = _effect.Parameters["ViewProjection"];
            _timeEP = _effect.Parameters["Time"];
            _spriteTextureSizeEP = _effect.Parameters["SpriteTextureSize"];
            _spriteTextureRegionSizeEP = _effect.Parameters["SpriteTextureRegionSize"];
            _spriteTextureRegionOffsetEP = _effect.Parameters["SpriteTextureRegionOffset"];
            _glowIntensitiyEP = _effect.Parameters["GlowIntensitiy"];
        }
        public void Configure(in Matrix viewProjection, in float time, in SizeF textureSize, in SizeF regionSize, in Vector2 regionOffset, in float glowIntensity)
        {
            if (_viewProjection != viewProjection)
            {
                _viewProjection = viewProjection;
                _viewProjectionEP.SetValue(viewProjection);
            }
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
            if (_regionSize != regionSize)
            {
                _regionSize = regionSize;
                _spriteTextureRegionSizeEP.SetValue(regionSize);
            }
            if (_regionOffset != regionOffset)
            {
                _regionOffset = regionOffset;
                _spriteTextureRegionOffsetEP.SetValue(regionOffset);
            }
            if (_glowIntensity != glowIntensity)
            {
                _glowIntensity = glowIntensity;
                _glowIntensitiyEP.SetValue(glowIntensity);
            }
        }
    }
}
