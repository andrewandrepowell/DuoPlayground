using Arch.Core;
using Arch.System;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Pow.Systems
{
    internal class PhysicsSystem : BaseSystem<Arch.Core.World, GameTime>
    {
        private const float _period = (float)1 / 60;
        private float _time = _period;
        private readonly nkast.Aether.Physics2D.Dynamics.World _physicsWorld;
        public PhysicsSystem(Arch.Core.World ecsWorld, nkast.Aether.Physics2D.Dynamics.World physicsWorld) : base(ecsWorld) 
        {
            _physicsWorld = physicsWorld;
        }
        public override void Update(in GameTime t)
        {
            while (_time < 0)
            {
                _physicsWorld.Step(_period);
                _time += _period;
            }
            _time -= t.GetElapsedSeconds();
            base.Update(t);
        }
    }
}
