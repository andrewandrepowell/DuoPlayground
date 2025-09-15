using DuoGum.Components;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class TitleMenu : GumObject
    {
        private titleView _view;
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            {
                _view = new titleView();
                GumManager.Initialize(_view.Visual);
                GumManager.Position = GumManager.Origin;
                GumManager.Layer = Layers.MidSky;
                GumManager.PositionMode = PositionModes.Screen;
            }
        }
    }
}
