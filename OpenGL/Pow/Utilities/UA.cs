using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pow.Utilities.Control;
using Microsoft.Xna.Framework;
using System;
using MonoGame.Extended;

namespace Pow.Utilities.UA
{
    public interface IUserAction
    {
        public void UpdateUserAction(int actionId, ButtonStates buttonState, float strength);
    }
    public class UAManager(UAGenerator parent) : IControl
    {
        private readonly UAGenerator _parent = parent;
        private bool _initialized = false;
        private IUserAction _userAction;
        private readonly Dictionary<Directions, int> _prevThumbstickActionIds = [];
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
        public Buttons[] ControlButtons
        {
            get
            {
                Debug.Assert(_initialized);
                return _parent.Buttons;
            }
        }
        public Directions[] ControlThumbsticks
        {
            get
            {
                Debug.Assert(_initialized);
                return _parent.Thumbsticks;
            }
        }
        public void UpdateControl(ButtonStates buttonState, Keys key)
        {
            Debug.Assert(_initialized);
            _userAction.UpdateUserAction(
                actionId: _parent.GetAction(key),
                buttonState: buttonState,
                strength: (buttonState == ButtonStates.Pressed) ? 1 : 0);
        }

        public void UpdateControl(ButtonStates buttonState, Buttons button)
        {
            Debug.Assert(_initialized);
            _userAction.UpdateUserAction(
                actionId: _parent.GetAction(button),
                buttonState: buttonState,
                strength: (buttonState == ButtonStates.Pressed)?1:0);
        }
        public void UpdateControl(Directions thumbstick, Vector2 position)
        {
            Debug.Assert(_initialized);
            var node = _parent.GetAction(thumbstick, position);
            var prevThumbstickActionId = _prevThumbstickActionIds.GetValueOrDefault(thumbstick, -1);
            if (prevThumbstickActionId >= 0 && prevThumbstickActionId != node.ActionId)
                _userAction.UpdateUserAction(
                    actionId: prevThumbstickActionId,
                    buttonState: ButtonStates.Released,
                    strength: 0);
            if (node.ActionId >= 0)
                _userAction.UpdateUserAction(
                    actionId: node.ActionId,
                    buttonState: ButtonStates.Pressed,
                    strength: node.Strength);
            _prevThumbstickActionIds[thumbstick] = node.ActionId;
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
        private Directions[] _thumbsticks;
        private List<ThumbstickConfig> _thumbstickConfigs = [];
        internal readonly record struct ThumbstickNode(int ActionId, float Strength);
        private record ThumbstickConfig(int ActionId, Directions Thumbstick, Vector2 Target);
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
        internal Directions[] Thumbsticks
        {
            get
            {
                Debug.Assert(_initialized);
                return _thumbsticks;
            }
        }
        public void Configure(int actionId)
        {
            Debug.Assert(!_initialized);
            Debug.Assert(!_actions.Contains(actionId));
            Debug.Assert(actionId >= 0);
            _actions.Add(actionId);
        }
        public void Configure(int actionId, Keys key)
        {
            Debug.Assert(_actions.Contains(actionId));
            _keyActionMap[key] = actionId;
            _keys = _keyActionMap.Keys.ToArray();
        }
        public void Configure(int actionId, Buttons button)
        {
            Debug.Assert(_actions.Contains(actionId));
            _buttonActionMap[button] = actionId;
            _buttons = _buttonActionMap.Keys.ToArray();
        }
        public void Configure(int actionId, Directions thumbstick, Vector2 target)
        {
            Debug.Assert(_actions.Contains(actionId));
            _thumbstickConfigs.RemoveAll(x=>x.ActionId == actionId);
            var configNode = new ThumbstickConfig(ActionId: actionId, Thumbstick: thumbstick, Target: target);
            _thumbstickConfigs.Add(configNode);
            _thumbsticks = _thumbstickConfigs.Select(x => x.Thumbstick).ToHashSet().ToArray();
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
        internal ThumbstickNode GetAction(Directions thumbstick, Vector2 position)
        {
            Debug.Assert(_initialized);
            if (position.EqualsWithTolerence(Vector2.Zero))
                return new(ActionId: -1, Strength: 0);
            int actionId = -1;
            float squareDistance = float.MaxValue;
            float strength = 0;
            foreach (var config in _thumbstickConfigs)
            {
                var configSquareDistasnce = (config.Target - position).LengthSquared();
                if (configSquareDistasnce < squareDistance)
                {
                    actionId = config.ActionId;
                    squareDistance = configSquareDistasnce;
                    strength = config.Target.Dot(position);
                }
            }
            return new(ActionId: actionId, Strength: strength);
        }
    }
}
