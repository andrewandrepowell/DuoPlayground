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
using Microsoft.Xna.Framework;

namespace Duo.Managers
{
    internal abstract class DuoObject : Environment
    {
        private AnimationManager _animationManager;
        private PhysicsManager _physicsManager;
        protected AnimationManager AnimationManager => _animationManager;
        protected PhysicsManager PhysicsManager => _physicsManager;
        public virtual Vector2 Position
        {
            get => _physicsManager.Body.Position * Globals.PixelsPerMeter;
            set => throw new NotImplementedException();
        }
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            _animationManager = Entity.Get<AnimationComponent>().Manager;
            _physicsManager = Entity.Get<PhysicsComponent>().Manager;
            var body = _physicsManager.Body;
            body.Tag = this;
        }
    }
}
