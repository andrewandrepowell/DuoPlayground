﻿using Arch.System;
using Arch.Core;
using Microsoft.Xna.Framework;
using Pow.Components;

namespace Pow.Systems
{
    internal class PositionSystem : BaseSystem<World, GameTime>
    {
        private readonly QueryDescription _allAnimationPosition;
        private readonly QueryDescription _allAnimationPhysics;
        private readonly ForEach _setAnimationPosition;
        private readonly ForEach _setAnimationPhysics;
        private void SetAnimationPosition(in Entity entity)
        {
            var animationComponent = World.Get<AnimationComponent>(entity);
            var positionComponent = World.Get<PositionComponent>(entity);
            animationComponent.Manager.Position = positionComponent.Vector;
        }
        private void SetAnimationPhysics(in Entity entity)
        {
            var animationManager = World.Get<AnimationComponent>(entity).Manager;
            var physicsBody = World.Get<PhysicsComponent>(entity).Manager.Body;
            animationManager.Position = physicsBody.Position;
            animationManager.Rotation = physicsBody.Rotation;
        }
        public override void Update(in GameTime t)
        {
            World.ParallelQuery(_allAnimationPosition, _setAnimationPosition);
            World.ParallelQuery(_allAnimationPhysics, _setAnimationPhysics);
            base.Update(t);
        }
        public PositionSystem(World world) : base(world)
        {
            _allAnimationPosition = new QueryDescription().WithAll<AnimationComponent, PositionComponent>();
            _allAnimationPhysics = new QueryDescription().WithAll<AnimationComponent, PhysicsComponent>();
            _setAnimationPosition = new((Entity entity) => SetAnimationPosition(in entity));
            _setAnimationPhysics = new((Entity entity) => SetAnimationPhysics(in entity));
        }
    }
}
