using Arch.Core;
using Arch.System;
using Arch.Core.Extensions;
using Microsoft.Xna.Framework;
using Pow.Components;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.Core.Utils;

namespace Pow.Systems
{
    internal class InitializeSystem : BaseSystem<World, GameTime>
    {
        private readonly QueryDescription _queryDescription = new QueryDescription().WithAll<StatusComponent>();
        private readonly ForEach _initializeComponents;
        private void InitializeComponents(in Entity entity)
        {
            ref var statusComponent = ref entity.Get<StatusComponent>();
            if (statusComponent.State != EntityStates.Initializing)
                return;
            var componentTypes = World.GetComponentTypes(entity);
            foreach (ref var componentType in componentTypes.AsSpan())
            {
                if (World.Get(entity, componentType) is IEntityInitialize entityInitialize)
                {
                    entityInitialize.Initialize(in entity);
                    World.Set(entity, (object)entityInitialize);
                }
            }
            statusComponent.State = EntityStates.Running;
        }
        public InitializeSystem(World world) : base(world)
        {
            _initializeComponents = new((Entity entity) => InitializeComponents(in entity));
        }
        public override void Update(in GameTime t)
        {
            World.Query(_queryDescription, _initializeComponents);
            base.Update(t);
        }
    }
}
