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
            _characterPhysics.Initialize(
                body: body, 
                boxes: Boxes,
                serviceInteractableContact: ServiceInteractableContact);
            _characterPhysics.Position = node.Vertices.Average() + node.Position;
            _characterPhysics.Flying = Flying;
            Direction = Enum.Parse<Directions>(node.Parameters.GetValueOrDefault("Direction", "Left"));
        }
        private void CleanupMovement()
        {
            _characterPhysics.Cleanup();
        }
        private void MovementUpdate()
        {
            if (Pow.Globals.GamePaused) 
                return;

            _characterPhysics.Update();

            var noLandAction = ((Action != Actions.Land) || !AnimationManager.Running);
            var noExpressAction = ((Action != Actions.Express) || (_expressNodes.Count == 0));
            if (_expressNodes.Count > 0 && Action != Actions.Express)
                UpdateAction(Actions.Express);
            else if (!Jumping && (Grounded || Flying) && !Moving && Action != Actions.Idle && Action != Actions.Fall && noLandAction && noExpressAction)
                UpdateAction(Actions.Idle);
            else if (!Jumping && (Grounded || Flying) && Moving && Action != Actions.Walk && Action != Actions.Fall && noLandAction && noExpressAction)
                UpdateAction(Actions.Walk);
            else if (!Flying && !Jumping && !Grounded && _characterPhysics.UpSpeed < 0 && Action != Actions.Fall && noLandAction && noExpressAction)
                UpdateAction(Actions.Fall);
            else if (!Flying && !Jumping && Grounded && Action == Actions.Fall && Action != Actions.Land && noExpressAction)
                UpdateAction(Actions.Land);
            else if (!Flying && Jumping && Action != Actions.Jump && noExpressAction)
                UpdateAction(Actions.Jump);
        }
        public bool MovingLeft => _characterPhysics.MovingLeft;
        public bool MovingRight => _characterPhysics.MovingRight;
        public bool Moving => _characterPhysics.Moving;
        public bool Jumping => _characterPhysics.Jumping;
        public bool Grounded => _characterPhysics.Grounded;
        public void MoveLeft(float strength = 1.0f)
        {
            _characterPhysics.MoveLeft(strength);
            Direction = Directions.Left;
        }
        public void UpdateLeft(float strength = 1.0f)
        {
            _characterPhysics.UpdateLeft(strength);
        }
        public void ReleaseLeft()
        {
            _characterPhysics.ReleaseMove();
        }
        public void MoveRight(float strength = 1.0f)
        {
            _characterPhysics.MoveRight(strength);
            Direction = Directions.Right;
        }
        public void UpdateRight(float strength = 1.0f)
        {
            _characterPhysics.UpdateRight(strength);
        }
        public void ReleaseRight()
        {
            _characterPhysics.ReleaseMove();
        }
        public void Jump()
        {
            _characterPhysics.Jump();
        }
        public void ReleaseJump()
        {
            _characterPhysics.ReleaseJump();
        }
        public void Bounce(Vector2 directionMagnitude)
        {
            _characterPhysics.Fling(directionMagnitude);
        }
    }
}
