using Arch.System;
using Arch.Core;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pow.Components;

namespace Pow.Systems
{
    internal class PositionSystem : BaseSystem<World, GameTime>
    {
        private readonly QueryDescription _allPositionAnimation = new QueryDescription().WithAll<PositionComponent, AnimationComponent>();
        private readonly ForEach _setAnimationPosition = new((Entity entity) => SetAnimationPosition(in entity));
        private static void SetAnimationPosition(in Entity entity)
        {
            var positionComponent = entity.Get<PositionComponent>();
            var animationComponent = entity.Get<AnimationComponent>();
            animationComponent.Manager.Position = positionComponent.Vector;
        }
        public override void Update(in GameTime t)
        {
            World.ParallelQuery(_allPositionAnimation, _setAnimationPosition);
            base.Update(t);
        }
        public PositionSystem(World world) : base(world)
        {
        }
    }
}
