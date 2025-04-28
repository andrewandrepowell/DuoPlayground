using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pow.Utilities;
using Microsoft.Xna.Framework;

namespace Duo.Managers
{
    internal partial class Character
    {
        private const float _movementBaseSpeed = 200000;
        private bool _movingLeft = false;
        private bool _movingRight = false;
        private void InitializeMovement()
        {
            _movingLeft = false;
            _movingRight = false;
            var body = PhysicsManager.Body;
            body.FixedRotation = true;
        }
        private void MovementUpdate()
        {
            if (Moving)
            {
                var body = PhysicsManager.Body;
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
