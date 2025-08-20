using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pow.Utilities.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Utilities.Shaders
{
    public class WindedEffect : BaseEffect
    {
        private readonly Effect _effect;
        private readonly EffectParameter _viewProjectionEP;
        private Matrix _viewProjection;
        public override Effect Effect => _effect;
        public WindedEffect()
        {
            _effect = Pow.Globals.Game.Content.Load<Effect>("effects/winded_0");
            _viewProjectionEP = _effect.Parameters["ViewProjection"];
        }
        public void Configure(in Matrix viewProjection)
        {
            if (_viewProjection != viewProjection)
            {
                _viewProjection = viewProjection;
                _viewProjectionEP.SetValue(viewProjection);
            }
        }
    }
}
