using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Collision.Shapes;
using Pow.Components;
using Pow.Utilities;
using Arch.Core;
using Arch.Core.Extensions;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Extended;

namespace Duo.Managers
{
    internal class Surface : Environment
    {
        public override void Initialize(Map.PolygonNode node)
        {
            base.Initialize(node);
            var body = Entity.Get<PhysicsComponent>().Manager.Body;
            body.BodyType = BodyType.Static;
            body.Mass = 1;
            var ghostVertices = node.Parameters
                .GetValueOrDefault("GhostVertices", "")
                .Split(",")
                .Select(token => token.Trim())
                .Where(token => !string.IsNullOrEmpty(token))
                .Select(token => int.Parse(token))
                .ToList();
            for (var i = 0; i < node.Vertices.Length; i++)
            {
                
                var vertex1Index = i;
                var vertex2Index = Pow.Utilities.Math.Mod(i + 1, node.Vertices.Length);
                if (ghostVertices.Contains(vertex1Index) || ghostVertices.Contains(vertex2Index))
                    continue;
                var edgeShape = new EdgeShape(node.Vertices[vertex1Index], node.Vertices[vertex2Index]);
                var vertex0Index = Pow.Utilities.Math.Mod(i - 1, node.Vertices.Length);
                if (ghostVertices.Contains(vertex0Index))
                {
                    edgeShape.Vertex0 = node.Vertices[vertex0Index];
                    edgeShape.HasVertex0 = true;
                }
                var vertex3Index = Pow.Utilities.Math.Mod(i + 2, node.Vertices.Length);
                if (ghostVertices.Contains(vertex3Index))
                {
                    edgeShape.Vertex3 = node.Vertices[vertex3Index];
                    edgeShape.HasVertex3 = true;
                }
                body.Add(new(edgeShape));
            }
            body.Position = node.Position;
            body.Tag = this;
        }
    }
}
