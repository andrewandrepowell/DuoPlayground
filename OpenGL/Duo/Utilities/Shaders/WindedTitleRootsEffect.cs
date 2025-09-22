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
    public class WindedTitleRootsEffect : BaseEffect
    {
        private readonly Effect _effect;
        private readonly EffectParameter _viewProjectionEP;
        private readonly EffectParameter _timeEP;
        private readonly EffectParameter _spriteTextureSizeEP;
        private readonly EffectParameter _spriteTextureRegionSizeEP;
        private readonly EffectParameter _spriteTextureRegionOffsetEP;
        private Matrix _viewProjection;
        private float _time;
        private SizeF _textureSize;
        private SizeF _regionSize;
        private Vector2 _regionOffset;

        public override Effect Effect => _effect;
        public WindedTitleRootsEffect()
        {
            _effect = Pow.Globals.Game.Content.Load<Effect>("effects/winded_3");
            _viewProjectionEP = _effect.Parameters["ViewProjection"];
            _timeEP = _effect.Parameters["Time"];
            _spriteTextureSizeEP = _effect.Parameters["SpriteTextureSize"];
            _spriteTextureRegionSizeEP = _effect.Parameters["SpriteTextureRegionSize"];
            _spriteTextureRegionOffsetEP = _effect.Parameters["SpriteTextureRegionOffset"];
        }
        public void Configure(
            in Matrix viewProjection,
            in float time,
            in SizeF textureSize,
            in Vector2 regionOffset,
            in SizeF regionSize)
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
            if (_regionOffset != regionOffset)
            {
                _regionOffset = regionOffset;
                _spriteTextureRegionOffsetEP.SetValue(regionOffset);
            }
            if (_regionSize != regionSize)
            {
                _regionSize = regionSize;
                _spriteTextureRegionSizeEP.SetValue(regionSize);
            }
        }
    }
}
