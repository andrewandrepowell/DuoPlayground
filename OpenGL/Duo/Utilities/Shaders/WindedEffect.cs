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
        private Matrix _viewProjection;
        private float _time;
        private SizeF _textureSize;
        private SizeF _regionSize;
        public override Effect Effect => _effect;
        public WindedEffect()
        {
            _effect = Pow.Globals.Game.Content.Load<Effect>("effects/winded_0");
            _viewProjectionEP = _effect.Parameters["ViewProjection"];
            _timeEP = _effect.Parameters["Time"];
            _spriteTextureSizeEP = _effect.Parameters["SpriteTextureSize"];
            _spriteTextureRegionSizeEP = _effect.Parameters["SpriteTextureRegionSize"];
        }
        public void Configure(in Matrix viewProjection, in float time, in SizeF textureSize, in SizeF regionSize)
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
                _spriteTextureRegionSizeEP?.SetValue(regionSize);
            }
        }
    }
}
