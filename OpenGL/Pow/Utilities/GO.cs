using Arch.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pow.Utilities.GO
{
    public interface IGOManager
    {
        public void Return();
    }
    public interface IGOGenerator<out T> where T : IGOManager
    {
        public void Initialize(int capacity);
        public T Acquire();
    }
    public abstract class GOCustomManager : IGOManager
    {
        private Action<GOCustomManager> _returnAction;
        private Entity _entity;
        private bool _initialized = false;
        private bool _acquired = false;
        internal void Initialize(Action<GOCustomManager> returnAction)
        {
            Debug.Assert(!_initialized);
            Debug.Assert(!_acquired);
            _returnAction = returnAction;
            _initialized = true;
        }
        internal void Initialize(in Entity entity)
        {
            Debug.Assert(_initialized);
            Debug.Assert(_acquired);
            _entity = entity;
            Initialize();
        }
        internal void Acquire()
        {
            Debug.Assert(_initialized);
            Debug.Assert(!_acquired);
            _acquired = true;
        }
        public ref Entity Entity => ref _entity;
        public void Return()
        {
            Debug.Assert(_initialized);
            Debug.Assert(_acquired);
            _returnAction(this);
            _acquired = false;
        }
        public virtual void Initialize()
        {
            Debug.Assert(_initialized);
            Debug.Assert(_acquired);
        }
        public virtual void Update()
        {
            Debug.Assert(_initialized);
            Debug.Assert(_acquired);
        }
    }
    public class GOCustomGenerator<T> : IGOGenerator<T> where T : GOCustomManager, new()
    {
        private readonly Action<GOCustomManager> _returnAction;
        private Queue<T> _managerPool = new();
        public bool _initialized = false;
        public GOCustomGenerator()
        {
            _returnAction = (GOCustomManager manager) => Return((T)manager);
        }
        public void Initialize(int capacity)
        {
            Debug.Assert(!_initialized);
            for (var i = 0; i < capacity; i++)
            {
                var manager = new T();
                manager.Initialize(_returnAction);
                _managerPool.Enqueue(manager);
            }
            _initialized = true;
        }
        public T Acquire()
        {
            Debug.Assert(_initialized);
            if (_managerPool.Count == 0) _managerPool.Enqueue(new T());
            var manager = _managerPool.Dequeue();
            manager.Acquire();
            return manager;
        }
        internal void Return(T manager)
        {
            Debug.Assert(_initialized);
            _managerPool.Enqueue(manager);
        }
    }
    public class GOGeneratorContainer
    {
        private readonly Dictionary<Type, IGOGenerator<IGOManager>> _generators = []; // maps manager type to generator.
        private readonly Dictionary<Type, ConfigNode> _configNodes = [];
        private bool _initialized = false;
        private record ConfigNode(int Capacity);
        public bool Initialized => _initialized;
        public void Add<T1, T2>(int capacity = 64)
            where T1 : IGOManager
            where T2 : IGOGenerator<T1>, new()
        {
            Debug.Assert(!_initialized);
            var type = typeof(T1);
            Debug.Assert(!_generators.ContainsKey(type));
            _generators.Add(type, (IGOGenerator<IGOManager>)new T2());
            _configNodes.Add(type, new(capacity));
        }
        public void Add<T>(int capacity = 64) where T : GOCustomManager, new() => Add<T, GOCustomGenerator<T>>(capacity);
        public T Acquire<T>() where T : IGOManager
        {
            Debug.Assert(_initialized);
            var type = typeof(T);
            return (T)_generators[type].Acquire();
        }
        public void Initialize()
        {
            Debug.Assert(!_initialized);
            foreach (var type in _generators.Keys)
            {
                var generator = _generators[type];
                var configNode = _configNodes[type];
                generator.Initialize(configNode.Capacity);
            }
            _initialized = true;
        }
    }
}
