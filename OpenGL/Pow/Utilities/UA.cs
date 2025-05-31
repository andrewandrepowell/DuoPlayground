using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pow.Utilities.Control;

namespace Pow.Utilities.UA
{
    public interface IUserAction
    {
        public void UpdateUserAction(int actionId, ButtonStates buttonState);
    }
    public class UAManager(UAGenerator parent) : IControl
    {
        private readonly UAGenerator _parent = parent;
        private bool _initialized = false;
        private IUserAction _userAction;
        public void Initialize(IUserAction userAction)
        {
            Debug.Assert(!_initialized);
            _userAction = userAction;
            _initialized = true;
        }

        public Keys[] ControlKeys
        {
            get
            {
                Debug.Assert(_initialized);
                return _parent.Keys;
            }
        }

        public void UpdateControl(ButtonStates buttonState, Keys key)
        {
            Debug.Assert(_initialized);
            _userAction.UpdateUserAction(
                actionId: _parent.GetAction(key),
                buttonState: buttonState);
        }
    }
    public class UAGenerator
    {
        private bool _initialized = false;
        private readonly List<int> _actions = [];
        private readonly Dictionary<Keys, int> _keyActionMap = [];
        private readonly Dictionary<Buttons, int> _buttonActionMap = [];
        private Keys[] _keys;
        private Buttons[] _buttons;
        internal Keys[] Keys
        {
            get
            {
                Debug.Assert(_initialized);
                return _keys;
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
        public void Configure(int actionId)
        {
            Debug.Assert(!_initialized);
            Debug.Assert(!_actions.Contains(actionId));
            _actions.Add(actionId);
        }
        public void Configure(int actionId, Keys key)
        {
            _keyActionMap[key] = actionId;
            _keys = _keyActionMap.Keys.ToArray();
        }
        public void Configure(int actionId, Buttons button)
        {
            _buttonActionMap[button] = actionId;
            _buttons = _buttonActionMap.Keys.ToArray();
        }
        public void Initalize()
        {
            Debug.Assert(!_initialized);
            Debug.Assert(_actions.All(actionId => _keyActionMap.ContainsValue(actionId)));
            Debug.Assert(_actions.All(actionId => _buttonActionMap.ContainsValue(actionId)));
            _initialized = true;
        }
        public UAManager Acquire()
        {
            Debug.Assert(_initialized);
            return new UAManager(this);
        }
        internal int GetAction(Keys key)
        {
            Debug.Assert(_initialized);
            return _keyActionMap[key];
        }
        internal int GetAction(Buttons button)
        {
            Debug.Assert(_initialized);
            return _buttonActionMap[button];
        }
    }
}
