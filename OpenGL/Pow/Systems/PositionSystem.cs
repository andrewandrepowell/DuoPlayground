using Arch.System;
using Arch.Core;
using Microsoft.Xna.Framework;
using Pow.Components;

namespace Pow.Systems
{
    internal class PositionSystem : BaseSystem<World, GameTime>
    {
        private readonly QueryDescription _allPositionAnimation;
        private readonly ForEach _setAnimationPosition;
        private void SetAnimationPosition(in Entity entity)
        {
            var positionComponent = World.Get<PositionComponent>(entity);
            var animationComponent = World.Get<AnimationComponent>(entity);
            animationComponent.Manager.Position = positionComponent.Vector;
        }
        public override void Update(in GameTime t)
        {
            World.ParallelQuery(_allPositionAnimation, _setAnimationPosition);
            base.Update(t);
        }
        public PositionSystem(World world) : base(world)
        {
            _allPositionAnimation = new QueryDescription().WithAll<PositionComponent, AnimationComponent>();
            _setAnimationPosition = new((Entity entity) => SetAnimationPosition(in entity));
        }
    }
}
