using Microsoft.Xna.Framework;
using Pow.Utilities;
using Pow.Utilities.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Utilities.Shaders
{
    public class WindedFeature : FeatureManager<WindedEffect>
    {
        private Layers _layer = Layers.MenuForeground;
        public override Layers Layer { get => _layer; set => _layer = value; }
        public override void UpdateEffect(in Matrix viewProjection)
        {
            base.UpdateEffect(in viewProjection);
            GetEffect().Configure(in viewProjection);
        }
    }
}
