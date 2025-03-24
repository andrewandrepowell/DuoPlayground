using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Microsoft.Xna.Framework;
using Pow.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Systems
{
    internal class DestroySystem : BaseSystem<World, GameTime>
    {
        private readonly QueryDescription _queryDescription = new QueryDescription().WithAll<StatusComponent>();
        private readonly ForEach _destroyComponents = new((Entity entity) => DestroyComponents(in entity));
        private static void DestroyComponents(in Entity entity)
        {
            ref var statusComponent = ref entity.Get<StatusComponent>();
            if (statusComponent.State != Utilities.EntityStates.Destroying)
                return;
            foreach (ref var componentType in entity.GetComponentTypes().AsSpan())
                if (entity.Get(componentType) is IDisposable disposable)
                    disposable.Dispose();
            statusComponent.State = Utilities.EntityStates.Destroyed;
        }
        public DestroySystem(World world) : base(world) 
        { 
        }
        public override void Update(in GameTime t)
        {
            World.Query(_queryDescription, _destroyComponents);
            base.Update(t);
        }
    }
}
