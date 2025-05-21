using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using nkast.Aether.Physics2D.Collision.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pow.Utilities;
using System.Diagnostics;
using Duo.Data;
using Duo.Managers;
using MonoGame.Extended;

namespace Duo.Utilities.Physics
{
    internal class Character
    {
        
        private readonly static BoxNode[] _boxNodes = 
        [
            new(BoxTypes.Collide, null), 
            new(BoxTypes.Ground, null), 
            new(BoxTypes.Wall, Directions.Left), 
            new(BoxTypes.Wall, Directions.Right) 
        ];
        private const float _baseGravity = 32e5f;
        private const float _baseMovement = 32e5f;
        private readonly static Vector2 _baseGroundNormal = -Vector2.UnitY;
        private bool _initialized = false;
        private Vector2 _groundNormal;
        private const float _groundNormalUpdateThreshold = 0.80f; // dot product of normals
        private Body _body;
        private readonly record struct BoxNode(BoxTypes BoxType, Directions? Direction);
        private readonly Dictionary<Fixture, BoxNode> _fixtureToBoxNodeMap = [];
        private readonly Dictionary<BoxNode, Fixture> _boxNodeToFixtureMap = [];
        private readonly record struct ContactNode(BoxNode BoxNode, Fixture OtherFixture);
        private readonly Dictionary<BoxNode, List<Fixture>> _fixtureBins = _boxNodes
            .Select(node => (node, new List<Fixture>()))
            .ToDictionary();
        private readonly Dictionary<BoxNode, List<Fixture>> _fixtureCollideBins = _boxNodes
            .Select(node => (node, new List<Fixture>()))
            .ToDictionary();
        private bool _moveLeft;
        private bool _moveRight;
        private const float _groundTimerMax = 0.25f; // seconds
        private float _groundedTimerValue;
        private const float _resetGroundNormalTimerMax = 0.5f; // seconds
        private float _resetGroundNormalTimerValue;
        public Vector2 Position
        {
            get
            {
                Debug.Assert(_initialized);
                return _body.Position;
            }
            set
            {
                Debug.Assert(_initialized);
                _body.Position = value;
            }
        }
        public bool Grounded
        {
            get
            {
                Debug.Assert(_initialized);
                return _groundedTimerValue > 0;
            }
        }
        public bool MovingLeft
        {
            get
            {
                Debug.Assert(_initialized);
                return _moveLeft;
            }
        }
        public bool MovingRight
        {
            get
            {
                Debug.Assert(_initialized);
                return _moveRight;
            }
        }
        public bool Moving
        {
            get
            {
                Debug.Assert(_initialized);
                return _moveLeft || _moveRight;
            }
        }
        public void Initialize(Body body, Boxes boxes)
        {
            Debug.Assert(!_initialized);
            {
                _body = body;
                body.FixedRotation = true;
                body.BodyType = BodyType.Dynamic;
                body.Mass = 1e-10f;
                var contactManager = body.World.ContactManager;
                contactManager.BeginContact += BeginContact;
                contactManager.EndContact += EndContact;
            }
            {
                Debug.Assert(body.FixtureList.Count == 0);
                var node = Globals.DuoRunner.BoxesGenerator.GetNode((int)boxes);
                void CreateFixture(
                    BoxTypes boxType,
                    Directions? direction,
                    Vector2[] vertices, 
                    bool isSensor)
                {
                    var shape = new PolygonShape(
                        vertices: new(vertices),
                        density: 1);
                    var fixture = new Fixture(shape);
                    fixture.IsSensor = isSensor;
                    var boxNode = new BoxNode(
                        BoxType: boxType,
                        Direction: direction);
                    _fixtureToBoxNodeMap[fixture] = boxNode;
                    _boxNodeToFixtureMap[boxNode] = fixture;
                    _body.Add(fixture);
                }
                CreateFixture(BoxTypes.Collide, null, node.Collide, false);
                CreateFixture(BoxTypes.Ground, null, node.Ground, true);
                CreateFixture(BoxTypes.Wall, Directions.Left, node.Walls[Directions.Left], true);
                CreateFixture(BoxTypes.Wall, Directions.Right, node.Walls[Directions.Right], true);
            }
            {
                _groundNormal = _baseGroundNormal;
            }
            {
                var collideBoxNode = new BoxNode(BoxTypes.Collide, null);
                foreach (ref var boxNode in _boxNodes.AsSpan())
                {
                    _fixtureBins[boxNode].Clear();
                    _fixtureCollideBins[boxNode].Clear();
                }
            }
            {
                _moveLeft = false;
                _moveRight = false;
                _groundedTimerValue = _groundTimerMax;
                _resetGroundNormalTimerValue = _resetGroundNormalTimerMax;
            }
            _initialized = true;
        }
        public void Cleanup()
        {
            Debug.Assert(_initialized);
            var contactManager = _body.World.ContactManager;
            contactManager.BeginContact -= BeginContact;
            contactManager.EndContact -= EndContact;
            _initialized = false;
        }
        private bool TryGetThisOtherFixtures(Contact contact, out Fixture thisFixture, out Fixture otherFixture)
        {
            thisFixture = null;
            otherFixture = null;
            if (contact.FixtureA.Body == _body)
            {
                thisFixture = contact.FixtureA;
                otherFixture = contact.FixtureB;
                return true;
            }
            if (contact.FixtureB.Body == _body)
            {
                thisFixture = contact.FixtureB;
                otherFixture = contact.FixtureA;
                return true;
            }
            return false;
        }
        private bool BeginContact(Contact contact)
        {
            Debug.Assert(_initialized);
            if (TryGetThisOtherFixtures(contact, out var thisFixture, out var otherFixture) && otherFixture.Body.Tag is Surface)
            {
                Debug.Assert(otherFixture.Shape is EdgeShape);
                var boxNode = _fixtureToBoxNodeMap[thisFixture];
                var bin = _fixtureBins[boxNode];
                Debug.Assert(!bin.Contains(otherFixture));
                _fixtureBins[boxNode].Add(otherFixture);
            }
            return true;
        }
        private void EndContact(Contact contact)
        {
            if (TryGetThisOtherFixtures(contact, out var thisFixture, out var otherFixture) && otherFixture.Body.Tag is Surface)
            {
                var boxNode = _fixtureToBoxNodeMap[thisFixture];
                var bin = _fixtureBins[boxNode];
                Debug.Assert(bin.Contains(otherFixture));
                _fixtureBins[boxNode].Remove(otherFixture);
            }
            Debug.Assert(_initialized);
        }
        public void MoveLeft()
        {
            Debug.Assert(_initialized);
            _moveLeft = true;
        }
        public void ReleaseMoveLeft()
        {
            Debug.Assert(_initialized);
            _moveLeft = false;
        }
        public void MoveRight()
        {
            Debug.Assert(_initialized);
            _moveRight = true;
        }
        public void ReleaseMoveRight()
        {
            Debug.Assert(_initialized);
            _moveRight = false;
        }
        public void Airborn()
        {
            _groundedTimerValue = 0;
            _groundNormal = _baseGroundNormal;
        }
        public void Jump()
        {
            Debug.Assert(_initialized);
        }
        public void ReleaseJump()
        {
            Debug.Assert(_initialized);
        }
        public void Update()
        {
            Debug.Assert(_initialized);

            var timeElapsed = Pow.Globals.GameTime.GetElapsedSeconds();
            var collideBoxNode = new BoxNode(BoxTypes.Collide, null);
            var groundBoxNode = new BoxNode(BoxTypes.Ground, null);
            var rightSpeed = _body.LinearVelocity.Dot(_groundNormal.PerpendicularCounterClockwise());
            var horizontalSpeed = System.Math.Abs(rightSpeed);

            // Update the fixture collide bins.
            // The collide bins indicate surface contacts on both collider and ground fixtures.
            {
                foreach (ref var boxNode in _boxNodes.AsSpan())
                    _fixtureCollideBins[boxNode].Clear();
                foreach (var otherFixture in _fixtureBins[collideBoxNode])
                    foreach (ref var boxNode in _boxNodes.AsSpan())
                        if (_fixtureBins[boxNode].Contains(otherFixture))
                            _fixtureCollideBins[boxNode].Add(otherFixture);
            }

            // Handle the ground state.
            {
                // Reset logic.
                var resetOccurred = _fixtureCollideBins[groundBoxNode].Count > 0;
                if (resetOccurred)
                {
                    _groundedTimerValue = _groundTimerMax;
                    resetOccurred = true;
                }
 
                // Ground timer logic.
                if (!resetOccurred && Grounded)
                {
                    _groundedTimerValue -= timeElapsed;
                    if (!Grounded)
                        Airborn();
                }

                // Ground normal logic.
                if (Grounded)
                {
                    var count = 0;
                    var total = Vector2.Zero;
                    foreach (var fixture in _fixtureBins[groundBoxNode])
                    {
                        var surface = (Surface)fixture.Body.Tag;
                        var fixtureNode = surface.GetFixtureNode(fixture);
                        var normal = fixtureNode.Normal;
                        var product = _groundNormal.Dot(normal);
                        if (_groundNormalUpdateThreshold <= product)
                        {
                            count++;
                            total += normal;
                        }
                    }
                    if (count > 0)
                    {
                        var average = total / count;
                        var targetNormal = Vector2.Normalize(average);
                        {
                            var curRads = (float)System.Math.Atan2(_groundNormal.Y, _groundNormal.X);
                            var tarRads = (float)System.Math.Atan2(targetNormal.Y, targetNormal.X);
                            var difRads = Pow.Utilities.Math.AngleDifference(curRads, tarRads);
                            var updRads = difRads * timeElapsed * horizontalSpeed * 0.25f;
                            var newRads = curRads + updRads;
                            var newNorm = new Vector2(
                                x: (float)System.Math.Cos(newRads),
                                y: (float)System.Math.Sin(newRads));
                            _groundNormal = newNorm;
                        }  
                    }
                }
            }

            _body.Rotation = (float)System.Math.Atan2(_groundNormal.Y, _groundNormal.X) + MathHelper.PiOver2;

            // Apply the constant forces
            {
                // gravity
                {
                    var speedValue = System.Math.Min(1, horizontalSpeed / 200);
                    var weightedDir = -(speedValue * _groundNormal + (1 - speedValue) * _baseGroundNormal);
                    var direction = weightedDir.EqualsWithTolerence(Vector2.Zero) ? Vector2.Zero : weightedDir.NormalizedCopy();
                    var force = direction * _baseGravity;
                    _body.ApplyForce(force);
                }

                if (_moveLeft)
                {
                    var direction = _groundNormal.PerpendicularClockwise();
                    var force = direction * _baseMovement;
                    _body.ApplyForce(force);
                }

                if (_moveRight)
                {
                    var direction = _groundNormal.PerpendicularCounterClockwise();
                    var force = direction * _baseMovement;
                    _body.ApplyForce(force);
                }
            }

            
        }
    }
}
