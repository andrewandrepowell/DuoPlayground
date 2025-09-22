using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Pow.Utilities;
using Pow.Utilities.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Utilities.Shaders
{
    public class WindedTitleRootsFeature : FeatureManager<WindedTitleRootsEffect>
    {
        private Layers _layer;
        public override Layers Layer { get => _layer; set => _layer = value; }
        public override void UpdateEffect(in Matrix viewProjection)
        {
            base.UpdateEffect(in viewProjection);
            GetEffect().Configure(
                viewProjection: in viewProjection,
                time: (float)Pow.Globals.GameTime.TotalGameTime.TotalSeconds,
                textureSize: new SizeF(
                    width: Parent.Texture.Width,
                    height: Parent.Texture.Height),
                regionOffset: Parent.Region.Location.ToVector2(),
                regionSize: Parent.Region.Size);
        }
    }
}
