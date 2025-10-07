using Arch.Core;
using Arch.Core.Extensions;
using Arch.Core.Utils;
using Arch.System;
using Microsoft.Xna.Framework;
using Pow.Components;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Systems
{
    internal class DestroySystem : BaseSystem<World, GameTime>
    {
        private readonly QueryDescription _queryDescription = new QueryDescription().WithAll<StatusComponent>();
        private readonly ForEach _destroyComponents;
        private readonly Dictionary<ComponentType, IDispose> _componentNodes = [];
        private bool _initialized = false;
        private interface IDispose
        {
            public void Dispose(in Entity entity);
        }
        private record ComponentNode<T>(World World) : IDispose where T : IDisposable
        {
            public void Dispose(in Entity entity)
            {
                ref var component = ref World.Get<T>(entity);
                component.Dispose();
            }
        }
        private void DestroyComponents(in Entity entity)
        {
            ref var statusComponent = ref entity.Get<StatusComponent>();
            if (statusComponent.State != EntityStates.Destroying)
                return;
            var componentTypes = World.GetComponentTypes(entity);
            for (var i = componentTypes.Length - 1; i >= 0; i--) 
                if (_componentNodes.TryGetValue(componentTypes[i], out var componentNode))
                    componentNode.Dispose(in entity);
            statusComponent.State = EntityStates.Running;
        }
        public void Add<T>() where T : IDisposable
        {
            Debug.Assert(!_initialized);
            var componentType = (ComponentType)typeof(T);
            Debug.Assert(!_componentNodes.ContainsKey(componentType));
            _componentNodes.Add(componentType, new ComponentNode<T>(World));
        }
        public DestroySystem(World world) : base(world) 
        {
            _destroyComponents = new((Entity entity) => DestroyComponents(in entity));
        }
        public override void Initialize()
        {
            Debug.Assert(!_initialized);
            base.Initialize();
            _initialized = true;
        }
        public override void Update(in GameTime t)
        {
            Debug.Assert(_initialized);
            World.Query(_queryDescription, _destroyComponents);
            base.Update(t);
        }
    }
}
