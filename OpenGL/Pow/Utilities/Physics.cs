using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nkast.Aether.Physics2D.Dynamics;
using Pow.Utilities.GO;

namespace Pow.Utilities.Physics
{
    public class PhysicsManager(PhysicsGenerator parent) : IGOManager
    {
        private readonly PhysicsGenerator _parent = parent;
        private readonly Body _body = new();
        private bool _acquired = false;
        public Body Body => _body;
        internal void Acquire()
        {
            Debug.Assert(!_acquired);
            _acquired = true;
        }
        public void Return()
        {
            Debug.Assert(_acquired);
            _parent.Return(this);
            _acquired = false;
        }
    }
    public class PhysicsGenerator(World world) : IGOGenerator<PhysicsManager>
    {
        private readonly World _world = world;
        private readonly Queue<PhysicsManager> _managerPool = [];
        private bool _initialized = false;
        private void Create() => _managerPool.Enqueue(new(this));
        public void Initialize(int capacity = 64)
        {
            Debug.Assert(!_initialized);
            for (var i = 0; i < capacity; i++) Create();
            _initialized = true;
        }
        public PhysicsManager Acquire()
        {
            Debug.Assert(_initialized);
            if (_managerPool.Count == 0) Create();
            var manager = _managerPool.Dequeue();
            _world.AddAsync(manager.Body);
            manager.Acquire();
            return manager;
        }
        internal void Return(PhysicsManager manager)
        {
            Debug.Assert(_initialized);
            _world.RemoveAsync(manager.Body);
            _managerPool.Enqueue(manager);
        }
    }
}
