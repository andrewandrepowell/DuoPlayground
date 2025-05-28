using DuoGum.Components;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class MainMenu : GumObject
    {
        private mainView _view;
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            _view = new mainView();
            GumManager.Initialize(_view.Visual);
            GumManager.Position = GumManager.Origin;
            GumManager.Layer = Layers.Menu;
            GumManager.PositionMode = Pow.Utilities.Gum.GumManager.PositionModes.Screen;
        }
    }
}
