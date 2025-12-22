using DuoGum.Components;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers;

internal class Dialogue : GumObject
{
    public override void Initialize(PolygonNode node)
    {
        base.Initialize(node);
        var view = new dialogueView();
        GumManager.Initialize(view.Visual);
        GumManager.Position = GumManager.Origin;
        GumManager.Layer = Layers.Interface;
        GumManager.PositionMode = PositionModes.Screen;
    }
}
