using Arch.Core.Extensions;
using Duo.Data;
using Duo.Utilities.Shaders;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using Pow.Components;
using Pow.Utilities;
using Pow.Utilities.Animations;
using Pow.Utilities.Physics;
using Pow.Utilities.Shaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers;

internal abstract class Bouncer : DuoObject
{
    private const float _baseDensity = 1f;
    private readonly static BoxTypes[] _boxTypes =
    [
        BoxTypes.Collide,
        BoxTypes.Ceil,
    ];
    private Body _body;
    private Actions _action;
    private float _magnitude;
    private Vector2 _direction;
    private AnimationGroupManager _animationGroupManager;
    private readonly Dictionary<Fixture, BoxTypes> _fixtureToBoxNodeMap = [];
    private readonly Dictionary<BoxTypes, List<Fixture>> _fixtureBins = _boxTypes
    .Select(node => (node, new List<Fixture>()))
    .ToDictionary();
    private readonly Dictionary<BoxTypes, List<Fixture>> _fixtureCollideBins = _boxTypes
        .Select(node => (node, new List<Fixture>()))
        .ToDictionary();
    private bool _initialized;
    public enum Actions { Waiting, Bouncing }
    protected abstract IReadOnlyDictionary<Actions, int> ActionAnimationGroupMap { get; }
    protected abstract Boxes Boxes { get; }
    protected abstract void Initialize(AnimationGroupManager manager);
    protected virtual Layers Layer => Layers.Ground;
    protected virtual Color GlowColor => Color.Gold;
    public override Vector2 Position
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
    public Vector2 Direction
    {
        get
        {
            Debug.Assert(_initialized);
            return _direction;
        }
    }
    public float Rotation
    {
        get
        {
            Debug.Assert(_initialized);
            return _body.Rotation;
        }
        set
        {
            Debug.Assert(_initialized);
            _body.Rotation = value;
            _direction = Pow.Utilities.Math.Vectorize(value - MathHelper.PiOver2);
        }
    }
    public float Magnitude
    {
        get
        {
            Debug.Assert(_initialized);
            return _magnitude;
        }
        set
        {
            Debug.Assert(_initialized);
            Debug.Assert(value >= 0);
            _magnitude = value;
        }
    }
    public Actions Action => _action;
    public float RotationDegrees
    {
        get => MathHelper.ToDegrees(Rotation);
        set => Rotation = MathHelper.ToRadians(value);
    }
    public virtual void Bounce()
    {
        Debug.Assert(_initialized);
        Debug.Assert(_action == Actions.Waiting);
        UpdateAction(Actions.Bouncing);
    }
    public virtual void FinishBounce()
    {
        Debug.Assert(_initialized);
        Debug.Assert(_action == Actions.Bouncing);
        UpdateAction(Actions.Waiting);
    }
    public virtual bool FinishedBouncing => Action == Actions.Bouncing && !_animationGroupManager.Running;
    public override void Initialize(PolygonNode node)
    {
        base.Initialize(node);
        {
            _body = PhysicsManager.Body;
            _body.FixedRotation = true;
            _body.BodyType = BodyType.Static;
            var contactManager = _body.World.ContactManager;
            contactManager.BeginContact += BeginContact;
            contactManager.EndContact += EndContact;
        }
        {
            var boxesNode = Globals.DuoRunner.BoxesGenerator.GetNode((int)Boxes);
            void CreateFixture(
                BoxTypes boxType,
                Vector2[] vertices,
                bool isSensor)
            {
                Debug.Assert(boxType == BoxTypes.Collide || boxType == BoxTypes.Ceil);
                var shape = new PolygonShape(
                    vertices: new(vertices.Select(
                        pixelPosition => pixelPosition / Globals.PixelsPerMeter)),
                    density: _baseDensity);
                var fixture = new Fixture(shape);
                fixture.Friction = 0.2f;
                fixture.Restitution = 0.0f;
                fixture.IsSensor = isSensor;
                _fixtureToBoxNodeMap[fixture] = boxType;
                _body.Add(fixture);
                fixture.CollisionCategories = Category.Cat1;
                fixture.CollidesWith = Category.Cat1;
            }
            CreateFixture(boxType: BoxTypes.Collide, vertices: boxesNode.Collide, isSensor: false);
            CreateFixture(boxType: BoxTypes.Ceil, vertices: boxesNode.Ceil, isSensor: true);
        }
        {
            foreach (ref var boxNode in _boxTypes.AsSpan())
            {
                _fixtureBins[boxNode].Clear();
                _fixtureCollideBins[boxNode].Clear();
            }
        }
        {
            _animationGroupManager = new(AnimationManager);
            Initialize(manager: _animationGroupManager);
            _animationGroupManager.Initialize();
        }
        {
            var animationManager = AnimationManager;
            animationManager.Layer = Layer;
        }
        {
            UpdateAction(Actions.Waiting);
        }
        {
            var floatFeature = AnimationManager.CreateFeature<FloatFeature, NullEffect>();
            floatFeature.Layer = Layer;
        }
        _initialized = true;
        {  
            Position = node.Vertices.Average() + node.Position;
            RotationDegrees = float.Parse(node.Parameters.GetValueOrDefault("RotationDegrees", "0.0"));
            Magnitude = float.Parse(node.Parameters.GetValueOrDefault("Magnitude", "1.0"));
        }
    }
    public override void Cleanup()
    {
        Debug.Assert(_initialized);
        var contactManager = _body.World.ContactManager;
        contactManager.BeginContact -= BeginContact;
        contactManager.EndContact -= EndContact;
        _initialized = false;
        base.Cleanup();
    }
    private void UpdateAction(Actions action)
    {
        if (ActionAnimationGroupMap.TryGetValue(action, out var groupId))
        {
            var animationManager = AnimationManager;
            _animationGroupManager.Play(groupId);
        }
        _action = action;
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
        // Check for contacts on the current body.
        // Ignore contacts with the sensor boxes of other bodies.
        // We currently only care about contacts with characters.
        if (TryGetThisOtherFixtures(contact, out var thisFixture, out var otherFixture) && !otherFixture.IsSensor &&
            (otherFixture.Body.Tag is Character))
        {
#if DEBUG
            if (otherFixture.Body.Tag is Character)
                Debug.Assert(otherFixture.Shape is PolygonShape);
#endif
            // Maintain knowledge of the fixture that contacted the current box (including sensors and collide box).
            var boxNode = _fixtureToBoxNodeMap[thisFixture];
#if DEBUG
            var bin = _fixtureBins[boxNode];
            Debug.Assert(!bin.Contains(otherFixture));
#endif
            _fixtureBins[boxNode].Add(otherFixture);
        }
        return true;
    }
    private void EndContact(Contact contact)
    {
        if (TryGetThisOtherFixtures(contact, out var thisFixture, out var otherFixture) && !otherFixture.IsSensor &&
            (otherFixture.Body.Tag is Character))
        {
            var boxNode = _fixtureToBoxNodeMap[thisFixture];
            var bin = _fixtureBins[boxNode];
            Debug.Assert(bin.Contains(otherFixture));
            _fixtureBins[boxNode].Remove(otherFixture);
        }
        Debug.Assert(_initialized);
    }
    public override void Update()
    {
        base.Update();

        // Bouncers are pausable.
        if (Pow.Globals.GamePaused) 
            return;

        Debug.Assert(_initialized);

        // Update the fixture collide bins.
        // The collide bins indicate surface contacts on both collider and ground fixtures.
        {
            foreach (var boxType in _boxTypes)
                _fixtureCollideBins[boxType].Clear();
            foreach (var otherFixture in _fixtureBins[BoxTypes.Collide])
                foreach (var boxNode in _boxTypes)
                    if (_fixtureBins[boxNode].Contains(otherFixture))
                        _fixtureCollideBins[boxNode].Add(otherFixture);
        }

        // Any characters who contact the bouncer will get bounced.
        if (_action == Actions.Waiting)
        {
            foreach (var otherFixture in _fixtureCollideBins[BoxTypes.Ceil])
            {
                if (otherFixture.Body.Tag is Character character)
                {
                    Bounce();
                    character.Bounce(directionMagnitude: _direction * _magnitude);
                }
            }
        }

        // Complete the bounce.
        if (FinishedBouncing)
            FinishBounce();

        // Play animation group.
        _animationGroupManager.Update();
    }
}
