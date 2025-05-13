using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Pow.Utilities.GO;

namespace Pow.Utilities.Control
{
    public enum ButtonStates { Pressed, Released }
    public interface IControl
    {
        public Keys[] ControlKeys { get; }
        public void UpdateControl(ButtonStates buttonState, Keys key);
    }
    public class ControlManager(ControlGenerator parent) : IGOManager
    {
        private readonly ControlGenerator _parent = parent;
        private IControl _control;
        private bool _acquired = false;
        private bool _initialized = false;
        internal IControl Control
        {
            get
            {
                Debug.Assert(_acquired);
                Debug.Assert(_initialized);
                return _control;
            }
        }
        internal void Acquire()
        {
            Debug.Assert(!_acquired);
            _acquired = true;
        }
        public void Initialize(IControl control)
        {
            Debug.Assert(_acquired);
            Debug.Assert(!_initialized);
            _control = control;
            _initialized = true;
        }
        public void Return()
        {
            Debug.Assert(_acquired);
            _parent.Return(this);
            _acquired = false;
            _initialized = false;
        }
    }
    public class ControlGenerator : IGOGenerator<ControlManager>
    {
        private readonly Queue<ControlManager> _managerPool = [];
        private bool _initialized = false;
        private void Create() => _managerPool.Enqueue(new(this));
        public void Initialize(int capacity = 64)
        {
            Debug.Assert(!_initialized);
            for (var i = 0; i < capacity; i++) Create();
            _initialized = true;
        }
        public ControlManager Acquire()
        {
            Debug.Assert(_initialized);
            if (_managerPool.Count == 0) Create();
            var manager = _managerPool.Dequeue();
            manager.Acquire();
            return manager;
        }
        internal void Return(ControlManager manager)
        {
            Debug.Assert(_initialized);
            _managerPool.Enqueue(manager);
        }
    }
}
