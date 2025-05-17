using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pow.Utilities;
using Microsoft.Xna.Framework;
using System.Xml.Linq;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using nkast.Aether.Physics2D.Collision.Shapes;
using System.Diagnostics;

namespace Duo.Managers
{
    internal partial class Character
    {
        private static Boxes.Node _boxesNode = null;
        private const float _movementGravity = 200000;
        private const float _movementBaseSpeed = 200000;
        private bool _movingLeft;
        private bool _movingRight;
        private static float GetRadius(Vector2[] vertices)
        {
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            foreach (ref var v in vertices.AsSpan())
            {
                if (v.X < min.X)
                    min.X = v.X;
                if (v.Y < min.Y)
                    min.Y = v.Y;
                if (v.X > max.X)
                    max.X = v.X;
                if (v.Y > max.Y)
                    max.Y = v.Y;
            }
            return System.Math.Min(max.X - min.X, max.Y - min.Y) / 2;
        }
        private void InitializeBoxes()
        {
            var body = PhysicsManager.Body;
            if (_boxesNode == null) 
                _boxesNode = Boxes.Load(BoxesAssetName);
            static void CreateFixture(Body body, Vector2[] vertices, bool isSensor)
            {
                var shape = new PolygonShape(
                    vertices: new(vertices),
                    density: 1);
                var fixture = new Fixture(shape);
                fixture.IsSensor = isSensor;
                body.Add(fixture);
            }
            CreateFixture(body, _boxesNode.Collide, false);
            CreateFixture(body, _boxesNode.Ground, true);
            CreateFixture(body, _boxesNode.Walls[Directions.Left], true);
            CreateFixture(body, _boxesNode.Walls[Directions.Right], true);
        }
        private void InitializeMovement(PolygonNode node)
        {
            _movingLeft = false;
            _movingRight = false;

            var body = PhysicsManager.Body;
            body.FixedRotation = true;
            body.BodyType = BodyType.Dynamic;
            body.Mass = 1;
            body.Position = node.Vertices.Average() + node.Position;
            InitializeBoxes();
            body.World.ContactManager.PostSolve += MovementPostSolve;
        }
        private void CleanupMovement()
        {
            var body = PhysicsManager.Body;
            body.World.ContactManager.PostSolve -= MovementPostSolve;
        }
        private void MovementPostSolve(Contact contact, ContactVelocityConstraint impulse)
        {
            var body = PhysicsManager.Body;
            if (contact.FixtureA.Body == body)
            {
                Console.WriteLine("Character is A");
            }
            if (contact.FixtureB.Body == body)
            {
                Console.WriteLine("Character is B");
                if (contact.FixtureA.Body.Tag is Surface)
                {
                    var surfaceShape = (EdgeShape)contact.FixtureA.Shape;
                    //Console.WriteLine($"Surface Shape: {surfaceShape.Vertex1}, {surfaceShape.Vertex2}");
                    //Debug.Assert(surfaceShape.Vertex1.Y == surfaceShape.Vertex2.Y);
                    //Debug.Assert(surfaceShape.Vertex1.X != surfaceShape.Vertex2.X);
                }
            }
        }
        private void MovementUpdate()
        {
            var body = PhysicsManager.Body;
            body.ApplyForce(new Vector2(0, _movementGravity));
            if (Moving)
            {
                if (_movingLeft) body.ApplyForce(new Vector2(-_movementBaseSpeed, 0));
                if (_movingRight) body.ApplyForce(new Vector2(+_movementBaseSpeed, 0));
            }
        }

        public bool MovingLeft => _movingLeft;
        public bool MovingRight => _movingRight;
        public bool Moving => _movingLeft || _movingRight;
        public void MoveLeft()
        {
            UpdateAction(Actions.Walk);
            Direction = Directions.Left;
            _movingLeft = true;
        }
        public void ReleaseLeft()
        {
            UpdateAction(Actions.Idle);
            _movingLeft = false;
        }
        public void MoveRight()
        {
            UpdateAction(Actions.Walk);
            Direction = Directions.Right;
            _movingRight = true;
        }
        public void ReleaseRight()
        {
            UpdateAction(Actions.Idle);
            _movingRight = false;
        }
    }
}
