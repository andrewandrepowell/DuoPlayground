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
using System.Collections.ObjectModel;

namespace Duo.Managers
{
    internal class Surface : Environment
    {
        public record FixtureNode(Vector2 Normal);
        private readonly Dictionary<Fixture, FixtureNode> _fixtureNodes = [];
        public FixtureNode GetFixtureNode(Fixture fixture) => _fixtureNodes[fixture];
        public override void Initialize(PolygonNode node)
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
            _fixtureNodes.Clear();
            for (var i = 0; i < node.Vertices.Length; i++)
            {
                var vertex1Index = i;
                var vertex2Index = Pow.Utilities.Math.Mod(i + 1, node.Vertices.Length);
                if (ghostVertices.Contains(vertex1Index) || ghostVertices.Contains(vertex2Index))
                    continue;
                var vertex1 = node.Vertices[vertex1Index];
                var vertex2 = node.Vertices[vertex2Index];
                var edgeShape = new EdgeShape(
                    start: vertex1 / Globals.PixelsPerMeter, 
                    end: vertex2 / Globals.PixelsPerMeter);
                var normal = Vector2.Normalize((vertex2 - vertex1).PerpendicularClockwise());
                var vertex0Index = Pow.Utilities.Math.Mod(i - 1, node.Vertices.Length);
                if (ghostVertices.Contains(vertex0Index))
                {
                    edgeShape.Vertex0 = node.Vertices[vertex0Index] / Globals.PixelsPerMeter;
                    edgeShape.HasVertex0 = true;
                }
                var vertex3Index = Pow.Utilities.Math.Mod(i + 2, node.Vertices.Length);
                if (ghostVertices.Contains(vertex3Index))
                {
                    edgeShape.Vertex3 = node.Vertices[vertex3Index] / Globals.PixelsPerMeter;
                    edgeShape.HasVertex3 = true;
                }
                var fixture = new Fixture(edgeShape);
                var fixtureNode = new FixtureNode(Normal: normal);
                _fixtureNodes.Add(fixture, fixtureNode);
                body.Add(fixture);
            }
            body.Position = node.Position / Globals.PixelsPerMeter;
            body.Tag = this;
        }
    }
}
