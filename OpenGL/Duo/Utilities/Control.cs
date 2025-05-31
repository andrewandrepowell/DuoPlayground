using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Utilities.Control
{
    internal interface IControl 
    {
        public void UpdateControl(Controls control, Pow.Utilities.Control.ButtonStates buttonState);
    }
    internal class ControlManager(ControlGenerator parent) : Pow.Utilities.Control.IControl
    {
        private readonly ControlGenerator _parent = parent;
        private bool _initialized = false;
        private IControl _control;
        public void Initialize(IControl control)
        {
            Debug.Assert(!_initialized);
            _control = control;
            _initialized = true;
        }

        public Keys[] ControlKeys
        {
            get
            {
                Debug.Assert(_initialized);
                return _parent.ControlKeys;
            }
        }

        public void UpdateControl(Pow.Utilities.Control.ButtonStates buttonState, Keys key)
        {
            Debug.Assert(_initialized);
            _control.UpdateControl(
                control: _parent.GetControl(key),
                buttonState: buttonState);
        }
    }
    internal class ControlGenerator
    {
        private bool _initialized = false;
        private readonly static Controls[] _controls = Enum.GetValues<Controls>();
        private readonly Dictionary<Keys, Controls> _keyControlMap = [];
        private readonly Dictionary<Buttons, Controls> _buttonControlMap = [];
        private Keys[] _controlKeys;
        private Buttons[] _buttons;
        internal Keys[] ControlKeys
        {
            get
            {
                Debug.Assert(_initialized);
                return _controlKeys;
            }
        }
        internal Buttons[] Buttons
        {
            get
            {
                Debug.Assert(_initialized);
                return _buttons;
            }
        }

        public void Configure(Controls control, Keys key)
        {
            _keyControlMap[key] = control;
            _controlKeys = _keyControlMap.Keys.ToArray();
        }
        public void Configure(Controls control, Buttons button)
        {
            _buttonControlMap[button] = control;
            _buttons = _buttonControlMap.Keys.ToArray();
        }
        public void Initalize()
        {
            Debug.Assert(!_initialized);
            Debug.Assert(_controls.All(control => _keyControlMap.ContainsValue(control)));
            Debug.Assert(_controls.All(control => _buttonControlMap.ContainsValue(control)));
            _initialized = true;
        }
        public ControlManager Acquire()
        {
            Debug.Assert(_initialized);
            return new ControlManager(this);
        }
        internal Controls GetControl(Keys key)
        {
            Debug.Assert(_initialized);
            return _keyControlMap[key];
        }
        internal Controls GetControl(Buttons button)
        {
            Debug.Assert(_initialized);
            return _buttonControlMap[button];
        }
    }
}
