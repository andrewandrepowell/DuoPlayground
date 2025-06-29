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
            new(BoxTypes.Wall, Directions.Right),
            new(BoxTypes.Vault, Directions.Left),
            new(BoxTypes.Vault, Directions.Right),
        ];
        private const float _baseGravity = 14;
        private const float _baseMovement = 20f;
        private const float _baseJump = 24;
        private const float _impulseJumpModifier = 0.15f;
        private const float _baseMass = 1f;
        private const float _baseLinearDamping = 15f;
        private const float _baseDensity = 1f;
        private const float _stillFriction = 3f;
        private const float _moveFriction = 1f;
        private const float _airFriction = 0f;
        private const float _maxHorizontalSpeed = 3;
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
        private Directions _moveDirection;
        private bool _moving;
        private float _moveForceMagnitude;
        private float _moveTimer;
        private float _moveTimerMax;
        private const float _moveMoveTimerMax = 2f;
        private const float _moveStillTimerMax = 1f;
        private const float _groundTimerMax = 0.25f; // seconds
        private float _groundedTimerValue;
        private Vector2 _jumpNormal;
        private float _jumpTimerValue;
        private const float _jumpTimerMax = 0.75f;
        private float _fallGravityTimer;
        private const float _fallGravityTimerMax = 3;
        private bool _vaultReady;
        private Median<float> _moveForceMedian = new(period: (float)1 / 30, amount: 16);
        private void UpdateCollideFriction(float friction)
        {
            var fixture = _boxNodeToFixtureMap[new(BoxTypes.Collide, null)];
            if (fixture.Friction == friction)
                return;
            fixture.Friction = friction;
            ContactEdge contactEdge = fixture.Body.ContactList;
            while (contactEdge != null) 
            {
                var contact = contactEdge.Contact;
                contact.ResetFriction();
                contactEdge = contactEdge.Next;
            }
        }
        public float UpSpeed
        {
            get
            {
                Debug.Assert(_initialized);
                return _body.LinearVelocity.Dot(_groundNormal) * Globals.PixelsPerMeter;
            }
        }
        public Vector2 Position
        {
            get
            {
                Debug.Assert(_initialized);
                return _body.Position * Globals.PixelsPerMeter;
            }
            set
            {
                Debug.Assert(_initialized);
                _body.Position = value / Globals.PixelsPerMeter;
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
                return _moving && _moveDirection == Directions.Left;
            }
        }
        public bool MovingRight
        {
            get
            {
                Debug.Assert(_initialized);
                return _moving && _moveDirection == Directions.Right;
            }
        }
        public bool Moving
        {
            get
            {
                Debug.Assert(_initialized);
                return _moving;
            }
        }
        public bool Jumping
        {
            get
            {
                Debug.Assert(_initialized);
                return _jumpTimerValue > 0;
            }
        }
        public void Initialize(Body body, Boxes boxes)
        {
            Debug.Assert(!_initialized);
            {
                _body = body;
                body.FixedRotation = true;
                body.BodyType = BodyType.Dynamic;
                body.Mass = _baseMass;
                body.LinearDamping = _baseLinearDamping;
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
                        vertices: new(vertices.Select(
                            pixelPosition => pixelPosition / Globals.PixelsPerMeter)),
                        density: _baseDensity);
                    var fixture = new Fixture(shape);
                    fixture.Friction = (boxType == BoxTypes.Collide)?_stillFriction:0;
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
                CreateFixture(BoxTypes.Vault, Directions.Left, node.Vaults[Directions.Left], true);
                CreateFixture(BoxTypes.Vault, Directions.Right, node.Vaults[Directions.Right], true);
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
                _moving = false;
                _moveDirection = Directions.Left;
                _moveForceMagnitude = 0;
                _moveTimer = 0;
                _moveTimerMax = _moveStillTimerMax;
                _groundedTimerValue = _groundTimerMax;
                _jumpTimerValue = 0;
                _fallGravityTimer = 0;
                _vaultReady = true;
                _moveForceMedian.Clear();
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
            Debug.Assert(!Moving);
            _moving = true;
            _moveDirection = Directions.Left;
            _moveTimerMax = _moveMoveTimerMax;
            _moveTimer = _moveTimerMax;
        }
        public void MoveRight()
        {
            Debug.Assert(_initialized);
            Debug.Assert(!Moving);
            _moving = true;
            _moveDirection = Directions.Right;
            _moveTimerMax = _moveMoveTimerMax;
            _moveTimer = _moveTimerMax;
        }
        public void ReleaseMove()
        {
            Debug.Assert(_initialized);
            Debug.Assert(Moving);
            _moving = false;
            _moveTimerMax = _moveStillTimerMax;
            _moveTimer = _moveTimerMax;
        }
        public void Jump()
        {
            Debug.Assert(_initialized);
            Debug.Assert(Grounded);
            Debug.Assert(!Jumping);
            _jumpTimerValue = _jumpTimerMax;
            _jumpNormal = _groundNormal;
            _groundedTimerValue = 0;
            var impulse = _jumpNormal * _baseJump * _impulseJumpModifier;
            _body.ApplyLinearImpulse(impulse);
        }
        public void ReleaseJump()
        {
            Debug.Assert(_initialized);
            Debug.Assert(Jumping);
            _jumpTimerValue = 0;
        }
        public void Update()
        {
            // https://info.sonicretro.org/Sonic_Physics_Guide
            Debug.Assert(_initialized);

            var timeElapsed = Pow.Globals.GameTime.GetElapsedSeconds();
            var collideBoxNode = new BoxNode(BoxTypes.Collide, null);
            var groundBoxNode = new BoxNode(BoxTypes.Ground, null);


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

            var resetToGroundOccurred = _fixtureCollideBins[groundBoxNode].Count > 0 && !Jumping;
            var runningIntoLeftWall = _fixtureCollideBins[new(BoxTypes.Wall, Directions.Left)].Count > 0 && MovingLeft;
            var runningIntoRightWall = _fixtureCollideBins[new(BoxTypes.Wall, Directions.Right)].Count > 0 && MovingRight;
            var runningIntoWall = runningIntoLeftWall || runningIntoRightWall;

            var rightSpeed = _body.LinearVelocity.Dot(_groundNormal.PerpendicularCounterClockwise());
            var horizontalSpeed = System.Math.Abs(rightSpeed);
            var speedValue = System.Math.Min(1, horizontalSpeed / _maxHorizontalSpeed);

            // Handle the ground state.
            {
                // Update ground timer.
                if (resetToGroundOccurred)
                    _groundedTimerValue = _groundTimerMax;
                else if (Grounded)
                    _groundedTimerValue -= timeElapsed;

                // Determine targert normal.
                // By default, the target normal is the base ground normal.
                var targetNormal = _baseGroundNormal;
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
                        targetNormal = Vector2.Normalize(average);
                    }
                }
            
                // Gradually update the ground normal to the target.
                {
                    var maxRadsPerSec = 1 / MathHelper.TwoPi * 64f;
                    var curRads = (float)System.Math.Atan2(_groundNormal.Y, _groundNormal.X);
                    var tarRads = (float)System.Math.Atan2(targetNormal.Y, targetNormal.X);
                    var difRads = Pow.Utilities.Math.AngleDifference(curRads, tarRads);
                    var updRads = System.Math.Min(difRads * System.Math.Max(horizontalSpeed, 0.5f) * 10f, maxRadsPerSec) * timeElapsed;
                    var newRads = curRads + updRads;
                    var newNorm = new Vector2(
                        x: (float)System.Math.Cos(newRads),
                        y: (float)System.Math.Sin(newRads));
                    _groundNormal = newNorm;
                }
            }

            // Update the rotation of the body.
            _body.Rotation = (float)System.Math.Atan2(_groundNormal.Y, _groundNormal.X) + MathHelper.PiOver2;

            var falling = !Grounded && !Jumping;

            // Update fall timer. Used in the gravity force calculations later.
            {
                if (resetToGroundOccurred)
                {
                    Debug.Assert(Grounded);
                    _fallGravityTimer = _fallGravityTimerMax;
                }
                else if (falling && _fallGravityTimer > 0)
                {
                    _fallGravityTimer -= timeElapsed;
                    if (_fallGravityTimer < 0)
                        _fallGravityTimer = 0;
                }
            }

            // Gravity and stick forces.
            {
                Vector2 force;
                if (Grounded && Moving && speedValue > 0.40f)
                    force = -_groundNormal * _baseGravity;
                else if (falling)
                    force = -_baseGroundNormal * _baseGravity * MathHelper.Lerp(6, 1, _fallGravityTimer / _fallGravityTimerMax);
                else
                    force = -_baseGroundNormal * _baseGravity;
                _body.ApplyForce(force);
            }

            // Update Jumping force.
            if (Jumping)
            {
                var direction = _jumpNormal;
                var force = direction * _baseJump * MathHelper.Lerp(0, 1, _jumpTimerValue / _jumpTimerMax);
                _body.ApplyForce(force);
                _jumpTimerValue -= timeElapsed;
            }

            // Perform vault.
            // https://bhopkins.net/pages/mmphysics/
            {
                if (resetToGroundOccurred)
                    _vaultReady = true;
                if (_vaultReady)
                {
                    // Determine if there's a vaultable surface on the left.
                    bool leftVaultableExists = false;
                    bool rightVaultableExists = false;
                    foreach (var fixture in _fixtureCollideBins[new(BoxTypes.Vault, Directions.Left)])
                    {
                        var surface = (Surface)fixture.Body.Tag;
                        var fixtureNode = surface.GetFixtureNode(fixture);
                        var normal = fixtureNode.Normal;
                        var product = _groundNormal.Dot(normal);
                        if (product <= 0.2f)
                        {
                            leftVaultableExists = true;
                            break;
                        }
                    }

                    // Determine if there's a vaultable surface on the right.
                    foreach (var fixture in _fixtureCollideBins[new(BoxTypes.Vault, Directions.Right)])
                    {
                        var surface = (Surface)fixture.Body.Tag;
                        var fixtureNode = surface.GetFixtureNode(fixture);
                        var normal = fixtureNode.Normal;
                        var product = _groundNormal.Dot(normal);
                        if (product <= 0.2f)
                        {
                            rightVaultableExists = true;
                            break;
                        }
                    }

                    // Determine if moving into no wall.
                    var movingLeftIntoNoWall = (_fixtureCollideBins[new(BoxTypes.Wall, Directions.Left)].Count == 0) && MovingLeft;
                    var movingRightIntoNoWall = (_fixtureCollideBins[new(BoxTypes.Wall, Directions.Right)].Count == 0) && MovingRight;

                    // Perform vault until ground reset occurs.
                    // Ground timer is reset to 0 to ground the character.
                    var vaultLeft = movingLeftIntoNoWall && leftVaultableExists;
                    var vaultRight = movingRightIntoNoWall && rightVaultableExists;
                    if (vaultLeft || vaultRight)
                    {
                        _body.ApplyLinearImpulse(_groundNormal * 2);
                        _groundedTimerValue = 0;
                        _vaultReady = false;
                    }
                }
            }

            // Update moving force.
            {
                var direction = (_moveDirection == Directions.Left) ? _groundNormal.PerpendicularClockwise() : _groundNormal.PerpendicularCounterClockwise();
                var timerRatio = _moveTimer / _moveTimerMax;
                float forceMagnitude;
                if (runningIntoWall)
                {
                    _moveForceMagnitude = 0;
                    forceMagnitude = 0;
                }
                else if (Moving)
                {
                    forceMagnitude = _baseMovement * MathHelper.Lerp(7.5f, 1, speedValue) * (1 - timerRatio);
                    _moveForceMagnitude = _moveForceMedian.Get();
                }
                else
                {
                    forceMagnitude = _moveForceMagnitude * timerRatio;
                }
                //Debug.Print($"runningIntoWall={runningIntoWall}, Moving={Moving}, forceMagnitude={_moveForceMedian.Get()}, timerRatio={timerRatio}, speedValue={speedValue}");
                var force = direction * forceMagnitude;
                _body.ApplyForce(force);
                _moveForceMedian.Update(timeElapsed, forceMagnitude);
                if (_moveTimer > 0)
                    _moveTimer -= timeElapsed;
                if (_moveTimer < 0)
                    _moveTimer = 0;
            }

            // Update friction
            {
                if (!Grounded)
                    UpdateCollideFriction(_airFriction);
                else if (Moving)
                    UpdateCollideFriction(_moveFriction);
                else
                    UpdateCollideFriction(_stillFriction);
            }
        }
    }
}
