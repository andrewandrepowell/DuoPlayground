using Pow.Utilities.Animations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Utilities.GO
{
    public abstract class GOCustomManager : IGOManager
    {
        private Action<GOCustomManager> _returnAction;
        private bool _initialized = false;
        private bool _acquired = false;
        internal void Initialize(Action<GOCustomManager> returnAction)
        {
            Debug.Assert(!_initialized);
            Debug.Assert(!_acquired);
            _returnAction = returnAction;
            _initialized = true;
        }
        internal void Acquire()
        {
            Debug.Assert(_initialized);
            Debug.Assert(!_acquired);
            _acquired = true;
        }
        public void Return()
        {
            Debug.Assert(_initialized);
            Debug.Assert(_acquired);
            _returnAction(this);
            _acquired = false;
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

    public interface IGOManager
    {
        public void Return();
    }
    public interface IGOGenerator<T> where T : IGOManager
    {
        public void Initialize(int capacity);
        public T Acquire();
    }
    public class GOContainer
    {
        private readonly Dictionary<Type, IGOGenerator<IGOManager>> _generators = [];
        private readonly Dictionary<Type, ConfigNode> _configNodes = [];
        private bool _initialized = false;
        private record ConfigNode(int Capacity);
        public bool Initialized => _initialized;
        public void Add<T>(int capacity = 64) where T : IGOGenerator<IGOManager>, new()
        {
            Debug.Assert(!_initialized);
            var type = typeof(T);
            Debug.Assert(!_generators.ContainsKey(type));
            _generators.Add(type, new T());
            _configNodes.Add(type, new(capacity));
        }
        public T Get<T>() where T : IGOGenerator<IGOManager>
        {
            Debug.Assert(_initialized);
            var type = typeof(T);
            return (T)_generators[type];
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
