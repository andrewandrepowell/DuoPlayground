using DuoGum;
using DuoGum.Components;
using Gum.Managers;
using GumRuntime;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class HUD : GumObject
    {
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            var hud = new hudView();
            GumManager.Initialize(hud.Visual);
            GumManager.Position = GumManager.Origin;
            GumManager.Layer = Layers.Interface;
            GumManager.PositionMode = PositionModes.Screen;
        }
    }
}
