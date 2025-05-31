using Arch.System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using EcsWorld = Arch.Core.World;
using PhysicsWorld = nkast.Aether.Physics2D.Dynamics.World;

namespace Pow.Systems
{
    internal class PhysicsSystem : BaseSystem<EcsWorld, GameTime>
    {
        private const float _maxPeriod = (float)1 / 15;
        private readonly PhysicsWorld _physicsWorld;
        public PhysicsSystem(EcsWorld ecsWorld, PhysicsWorld physicsWorld) : base(ecsWorld) 
        {
            _physicsWorld = physicsWorld;
        }
        public override void Update(in GameTime t)
        {
            base.Update(t);
            if (Globals.GamePaused) return;
            var timeElapsed = System.Math.Min(_maxPeriod, t.GetElapsedSeconds());
            _physicsWorld.Step(t.GetElapsedSeconds());
        }
    }
}
