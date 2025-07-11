using Arch.Core;
using Arch.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Pow.Components;
using Pow.Utilities;
using Pow.Utilities.Control;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Systems
{
    internal class ControlSystem : BaseSystem<World, GameTime>
    {
        private readonly QueryDescription _queryDescription = new QueryDescription().WithAll<ControlComponent>();
        private readonly ForEach _updateControl, _setPrevStates;
        private KeyboardState _keyboardState;
        private GamePadState _gamePadState;
        private Dictionary<Keys, ButtonStates> _keyPrevStates = [];
        private Dictionary<Buttons, ButtonStates> _buttonPrevStates = [];
        private Dictionary<Directions, Vector2> _thumbstickPrevPositions = [];
        private void UpdateControl(in Entity entity)
        {
            var controlManager = World.Get<ControlComponent>(entity).Manager;
            foreach (ref var key in controlManager.Control.ControlKeys.AsSpan())
            {
                var keyUp = _keyboardState.IsKeyUp(key);
                var keyDown = _keyboardState.IsKeyDown(key);
                var keyCurrState = (keyDown) ? ButtonStates.Pressed : ButtonStates.Released;
                Debug.Assert((keyUp && !keyDown) || (!keyUp && keyDown));
                var keyPrevState = _keyPrevStates.GetValueOrDefault(key, ButtonStates.Released);

                if (keyCurrState == ButtonStates.Pressed && keyPrevState == ButtonStates.Released)
                    controlManager.Control.UpdateControl(
                        buttonState: ButtonStates.Pressed,
                        key: key);
                else if (keyCurrState == ButtonStates.Released && keyPrevState == ButtonStates.Pressed)
                    controlManager.Control.UpdateControl(
                        buttonState: ButtonStates.Released,
                        key: key);
            }
            foreach (ref var button in controlManager.Control.ControlButtons.AsSpan())
            {
                var buttonUp = _gamePadState.IsButtonUp(button);
                var buttonDown = _gamePadState.IsButtonDown(button);
                var buttonCurrState = (buttonDown) ? ButtonStates.Pressed : ButtonStates.Released;
                Debug.Assert((buttonUp && !buttonDown) || (!buttonUp && buttonDown));
                var buttonPrevState = _buttonPrevStates.GetValueOrDefault(button, ButtonStates.Pressed);

                if (buttonCurrState == ButtonStates.Pressed && buttonPrevState == ButtonStates.Released)
                    controlManager.Control.UpdateControl(
                        buttonState: ButtonStates.Pressed,
                        button: button);
                else if (buttonCurrState == ButtonStates.Released && buttonPrevState == ButtonStates.Pressed)
                    controlManager.Control.UpdateControl(
                        buttonState: ButtonStates.Released,
                        button: button);
            }
            foreach (ref var thumbstick in controlManager.Control.ControlThumbsticks.AsSpan())
            {
                var position =
                    (thumbstick == Directions.Left) ? _gamePadState.ThumbSticks.Left :
                    (thumbstick == Directions.Right) ? _gamePadState.ThumbSticks.Right :
                    Vector2.Zero;
                var prevPosition = _thumbstickPrevPositions.GetValueOrDefault(thumbstick, Vector2.Zero);
                if (position != prevPosition)
                    controlManager.Control.UpdateControl(thumbstick, position);
            }
        }
        private void SetPrevStates(in Entity entity)
        {
            var controlManager = World.Get<ControlComponent>(entity).Manager;
            foreach (ref var key in controlManager.Control.ControlKeys.AsSpan())
            {
                var keyDown = _keyboardState.IsKeyDown(key);
                var keyCurrState = (keyDown) ? ButtonStates.Pressed : ButtonStates.Released;
                _keyPrevStates[key] = keyCurrState;
            }
            foreach (ref var button in controlManager.Control.ControlButtons.AsSpan())
            {
                var buttonDown = _gamePadState.IsButtonDown(button);
                var buttonCurrState = (buttonDown) ? ButtonStates.Pressed : ButtonStates.Released;
                _buttonPrevStates[button] = buttonCurrState;
            }
            foreach (ref var thumbstick in controlManager.Control.ControlThumbsticks.AsSpan())
            {
                var position =
                    (thumbstick == Directions.Left) ? _gamePadState.ThumbSticks.Left :
                    (thumbstick == Directions.Right) ? _gamePadState.ThumbSticks.Right :
                    Vector2.Zero;
                _thumbstickPrevPositions[thumbstick] = position;
            }
        }
        public ControlSystem(World world) : base(world)
        {
            _updateControl = new((Entity entity) => UpdateControl(in entity));
            _setPrevStates = new((Entity entity) => SetPrevStates(in entity));
        }
        public override void Update(in GameTime t)
        {
            _keyboardState = Keyboard.GetState();
            try
            {
                _gamePadState = GamePad.GetState(0);
            }
            catch (NotImplementedException)
            {
                _gamePadState = new();
            }
            World.Query(_queryDescription, _updateControl);
            World.Query(_queryDescription, _setPrevStates);
            base.Update(t);
        }
    }
}
