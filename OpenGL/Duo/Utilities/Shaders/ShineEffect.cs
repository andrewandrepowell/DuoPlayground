using Microsoft.Xna.Framework.Graphics;
using Pow.Utilities.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Utilities.Shaders
{
    public class ShineEffect : BaseEffect
    {
        private readonly Effect _effect;
        private readonly EffectParameter _timeEP;
        private float _time;
        public override Effect Effect => _effect;
        public ShineEffect()
        {
            _effect = Pow.Globals.Game.Content.Load<Effect>("effects/shine_0");
            _timeEP = _effect.Parameters["Time"];
        }
        public void Configure(float time)
        {
            if (_time != time)
            {
                _time = time;
                _timeEP.SetValue(time);
            }
        }
    }
}
