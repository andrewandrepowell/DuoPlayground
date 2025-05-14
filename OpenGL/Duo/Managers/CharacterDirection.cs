using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal partial class Character
    {
        private Directions _direction;
        protected void UpdateDirection()
        {
            AnimationManager.Direction = _direction;
        }

        private void InitializeDirection()
        {
            _direction = Directions.Left;
            UpdateDirection();
        }
        public Directions Direction
        {
            get => _direction;
            set
            {
                if (_direction == value) return;
                _direction = value;
                UpdateDirection();
            }
        }
    }
}
