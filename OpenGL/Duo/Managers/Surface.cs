using nkast.Aether.Physics2D.Dynamics;
using Pow.Components;
using Pow.Utilities;
using Arch.Core;
using Arch.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class Surface : Environment
    {
        public override void Initialize(Map.PolygonNode node)
        {
            var body = Entity.Get<PhysicsComponent>().Manager.Body;
            body.BodyType = BodyType.Static;
            body.Mass = 1;
            body.CreatePolygon(vertices: new(node.Vertices), density: 1);
            body.Position = node.Position;
        }
    }
}
