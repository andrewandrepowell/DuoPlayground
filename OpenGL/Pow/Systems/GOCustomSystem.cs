using Arch.Core;
using Arch.System;
using Microsoft.Xna.Framework;
using Pow.Utilities.GO;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pow.Components;

namespace Pow.Systems
{
    internal class GOCustomSystem : BaseSystem<World, GameTime>
    {
        private List<IUpdate> _updates = [];
        private bool _initialized = false;
        private record SystemNode<T>(World World) : IUpdate where T : GOCustomManager
        {
            private readonly QueryDescription _queryDescription = new QueryDescription().WithAll<GOCustomComponent<T>>();
            private readonly ForEach<GOCustomComponent<T>> _updateComponent = new((ref GOCustomComponent<T> component) => component.Manager.Update());
            public void Update() => World.Query(_queryDescription, _updateComponent);
        }
        public GOCustomSystem(World world) : base(world) { }
        public void Add<T>() where T : GOCustomManager
        {
            Debug.Assert(!_initialized);
            Debug.Assert(_updates.OfType<SystemNode<T>>().Count() == 0);
            _updates.Add(new SystemNode<T>(World));
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
            foreach (var update in _updates) update.Update();
            base.Update(t);
        }
    }
}
