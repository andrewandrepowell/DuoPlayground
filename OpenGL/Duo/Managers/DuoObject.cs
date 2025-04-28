using nkast.Aether.Physics2D.Dynamics;
using Pow.Utilities.Animations;
using Pow.Utilities.Physics;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.Core.Extensions;
using Pow.Components;

namespace Duo.Managers
{
    internal abstract class DuoObject : Environment
    {
        private AnimationManager _animationManager;
        private PhysicsManager _physicsManager;
        private Directions _direction;
        protected AnimationManager AnimationManager => _animationManager;
        protected PhysicsManager PhysicsManager => _physicsManager;
        public virtual Directions Direction { get => _direction; set => _direction = value; }
        public override void Initialize(Map.PolygonNode node)
        {
            _animationManager = Entity.Get<AnimationComponent>().Manager;
            _physicsManager = Entity.Get<PhysicsComponent>().Manager;
            _direction = Directions.Left;
        }
    }
}
