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
using System.Diagnostics;

namespace Pow.Systems
{
    internal class InitializeSystem : BaseSystem<World, GameTime>
    {
        private readonly QueryDescription _queryDescription = new QueryDescription().WithAll<StatusComponent>();
        private readonly ForEach _initializeComponents;
        private readonly Dictionary<ComponentType, IEntityInitialize> _componentNodes = [];
        private bool _initialized = false;
        private record ComponentNode<T>(World World) : IEntityInitialize where T : IEntityInitialize
        {
            public void Initialize(in Entity entity)
            {
                ref var component = ref World.Get<T>(entity);
                component.Initialize(entity);
            }
        }
        private void InitializeComponents(in Entity entity)
        {
            ref var statusComponent = ref entity.Get<StatusComponent>();
            if (statusComponent.State != EntityStates.Initializing)
                return;
            foreach (ref var componentType in World.GetComponentTypes(entity).AsSpan())
                if (_componentNodes.TryGetValue(componentType, out var componentNode))
                    componentNode.Initialize(in entity);
            statusComponent.State = EntityStates.Running;
        }
        public void Add<T>() where T : IEntityInitialize
        {
            Debug.Assert(!_initialized);
            var componentType = (ComponentType)typeof(T);
            Debug.Assert(!_componentNodes.ContainsKey(componentType));
            _componentNodes.Add(componentType, new ComponentNode<T>(World));
        }
        public InitializeSystem(World world) : base(world)
        {
            _initializeComponents = new((Entity entity) => InitializeComponents(in entity));
        }
        public override void Initialize()
        {
            base.Initialize();
            _initialized = true;
        }
        public override void Update(in GameTime t)
        {
            World.Query(_queryDescription, _initializeComponents);
            base.Update(t);
        }
    }
}
