using Arch.System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using EcsWorld = Arch.Core.World;
using PhysicsWorld = nkast.Aether.Physics2D.Dynamics.World;

namespace Pow.Systems
{
    internal class PhysicsSystem : BaseSystem<EcsWorld, GameTime>
    {
        private const float _period = (float)1 / 60;
        private float _time = _period;
        private readonly PhysicsWorld _physicsWorld;
        public PhysicsSystem(EcsWorld ecsWorld, PhysicsWorld physicsWorld) : base(ecsWorld) 
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
