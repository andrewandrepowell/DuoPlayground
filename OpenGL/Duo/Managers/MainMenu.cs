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
        private string _dimmerID;
        private mainView _view;
        private Dimmer _dimmer;
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            _view = new mainView();
            GumManager.Initialize(_view.Visual);
            GumManager.Position = GumManager.Origin;
            GumManager.Layer = Layers.Menu;
            GumManager.PositionMode = PositionModes.Screen;
            _dimmer = null;
            _dimmerID = node.Parameters.GetValueOrDefault("DimmerID", "Dimmer");
        }
        public override void Update()
        {
            if (_dimmer == null)
                _dimmer = Globals.DuoRunner.Environments.OfType<Dimmer>().Where(dimmer => dimmer.ID == _dimmerID).First();
        }
    }
}
