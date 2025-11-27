using Arch.Core;
using Arch.Core.Extensions;
using Duo.Data;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;
using Pow.Components;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class Surface : Environment
    {
        private Body _body;
        private Modes _mode;
        private static readonly ReadOnlyDictionary<MoveStates, MoveStates> _moveNextState = new(new Dictionary<MoveStates, MoveStates>() 
        {
            {  MoveStates.ToStart, MoveStates.ToEnd },
            {  MoveStates.ToEnd, MoveStates.ToStart },
        });
        private readonly Dictionary<MoveStates, Vector2> _moveDests = new();
        private float _moveDist;
        private MoveStates _moveState;
        private readonly Dictionary<Fixture, FixtureNode> _fixtureNodes = [];
        public enum Modes { Static, Move }
        public enum MoveStates { ToStart, ToEnd }
        public enum CollisionModes { None, Normal, OneWay }
        public record FixtureNode(Vector2 Normal, CollisionModes CollisionMode);
        public Modes Mode => _mode;
        public MoveStates MoveState
        {
            get
            {
                Debug.Assert(_mode == Modes.Move);
                return _moveState;
            }
        }
        public FixtureNode GetFixtureNode(Fixture fixture) => _fixtureNodes[fixture];
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            _mode = Enum.Parse<Modes>(node.Parameters.GetValueOrDefault("Mode", "Static"));
            {
                var body = Entity.Get<PhysicsComponent>().Manager.Body;
                _body = body;
                body.BodyType = _mode switch
                {
                    Modes.Static => BodyType.Static,
                    Modes.Move => BodyType.Kinematic,
                    _ => throw new NotImplementedException()
                };
                body.Mass = 0;
                body.LinearDamping = 15;
                var ghostVertices = node.Parameters
                    .GetValueOrDefault("GhostVertices", "")
                    .Split(",")
                    .Select(token => token.Trim())
                    .Where(token => !string.IsNullOrEmpty(token))
                    .Select(token => int.Parse(token))
                    .ToList();
                var origin = Vector2.Zero;
                if (node.Vertices.Length > 0)
                {
                    foreach (var vertex in node.Vertices.AsSpan())
                        origin += vertex;
                    origin /= node.Vertices.Length;
                }
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
                        start: (vertex1 - origin) / Globals.PixelsPerMeter,
                        end: (vertex2 - origin) / Globals.PixelsPerMeter);
                    var normal = Vector2.Normalize((vertex2 - vertex1).PerpendicularClockwise());
                    var vertex0Index = Pow.Utilities.Math.Mod(i - 1, node.Vertices.Length);
                    if (ghostVertices.Contains(vertex0Index))
                    {
                        edgeShape.Vertex0 = (node.Vertices[vertex0Index] - origin) / Globals.PixelsPerMeter;
                        edgeShape.HasVertex0 = true;
                    }
                    var vertex3Index = Pow.Utilities.Math.Mod(i + 2, node.Vertices.Length);
                    if (ghostVertices.Contains(vertex3Index))
                    {
                        edgeShape.Vertex3 = (node.Vertices[vertex3Index] - origin) / Globals.PixelsPerMeter;
                        edgeShape.HasVertex3 = true;
                    }
                    var collisionMode = Enum.Parse<CollisionModes>(node.Parameters.GetValueOrDefault($"CollisionMode{i}", "Normal"));
                    var fixture = new Fixture(edgeShape);
                    fixture.Friction = 0.2f;
                    fixture.Restitution = 0.0f;
                    var fixtureNode = new FixtureNode(
                        Normal: normal,
                        CollisionMode: collisionMode);
                    _fixtureNodes.Add(fixture, fixtureNode);
                    body.Add(fixture);
                    fixture.CollisionCategories = ((collisionMode == CollisionModes.Normal) ? Category.Cat1 : Category.None) | Category.Cat2;
                }
                body.Position = (node.Position + origin) / Globals.PixelsPerMeter;
                body.Tag = this;
            }
            {
                var moveComponents = node.Parameters
                    .GetValueOrDefault("Move", "0, 0")
                    .Split(",")
                    .Select(x => x.Trim())
                    .Select(x => float.Parse(x))
                    .ToArray();
                Debug.Assert(moveComponents.Length == 2);
                var move = new Vector2(x: moveComponents[0], y: moveComponents[1]);
                _moveDests[MoveStates.ToStart] = _body.Position * Globals.PixelsPerMeter;
                _moveDests[MoveStates.ToEnd] = _body.Position * Globals.PixelsPerMeter + move;
                _moveDist = (_moveDests[MoveStates.ToStart] - _moveDests[MoveStates.ToEnd]).Length();
                if (_mode == Modes.Move)
                    Debug.Assert(!_moveDist.EqualsWithTolerance(0));
                _moveState = MoveStates.ToEnd;
            }
            {
                var animationManager = Entity.Get<AnimationComponent>().Manager;
                animationManager.Play((int)Enum.Parse<Animations>(node.Parameters.GetValueOrDefault("Animation", "Pixel")));
                animationManager.Visibility = (_mode == Modes.Static) ? 0 : 1;
            }
        }
        public override void Update()
        {
            base.Update();
            if (Pow.Globals.GamePaused)
                return;
            if (_mode == Modes.Move)
            {
                var dest = _moveDests[_moveState];
                var vectorToDest = dest - _body.Position * Globals.PixelsPerMeter;
                var distanceToDest = vectorToDest.Length();
                if (distanceToDest <= 1)
                {
                    _moveState = _moveNextState[_moveState];
                    dest = _moveDests[_moveState];
                    vectorToDest = dest - _body.Position * Globals.PixelsPerMeter;
                    distanceToDest = vectorToDest.Length();
                }

                var timeElapsed = Pow.Globals.GameTime.GetElapsedSeconds();
                var v0 = System.Math.Min(distanceToDest, 32);
                var v1 = System.Math.Min(_moveDist - distanceToDest, 32);
                var v2 = System.Math.Max(System.Math.Min(v0, v1) / 32, 0.1f);

                var direction = vectorToDest / distanceToDest;
                _body.LinearVelocity = direction * v2 * timeElapsed * 50;
            }
        }
    }
}
