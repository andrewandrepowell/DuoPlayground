using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pow.Utilities;
using Duo.Utilities;
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
        private Utilities.Physics.Character _characterPhysics = new();
        private void InitializeMovement(PolygonNode node)
        {
            var body = PhysicsManager.Body;
            _characterPhysics.Initialize(body: body, Boxes);
            _characterPhysics.Position = node.Vertices.Average() + node.Position;
        }
        private void CleanupMovement()
        {
            _characterPhysics.Cleanup();
        }
        private void MovementUpdate()
        {
            _characterPhysics.Update();
        }
        public bool MovingLeft => _characterPhysics.MovingLeft;
        public bool MovingRight => _characterPhysics.MovingRight;
        public bool Moving => _characterPhysics.Moving;
        public void MoveLeft()
        {
            _characterPhysics.MoveLeft();
            Direction = Directions.Left;
            UpdateAction(Actions.Walk);
        }
        public void ReleaseLeft()
        {
            _characterPhysics.ReleaseMoveLeft();
            UpdateAction(Actions.Idle);
        }
        public void MoveRight()
        {
            _characterPhysics.MoveRight();
            Direction = Directions.Right;
            UpdateAction(Actions.Walk);
        }
        public void ReleaseRight()
        {
            _characterPhysics.ReleaseMoveRight();
            UpdateAction(Actions.Idle);
        }
    }
}
