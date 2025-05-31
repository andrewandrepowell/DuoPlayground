using Arch.Core;
using Arch.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Pow.Components;
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
        private Dictionary<Keys, ButtonStates> _keyPrevStates = [];
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
        }
        public ControlSystem(World world) : base(world)
        {
            _updateControl = new((Entity entity) => UpdateControl(in entity));
            _setPrevStates = new((Entity entity) => SetPrevStates(in entity));
        }
        public override void Update(in GameTime t)
        {
            _keyboardState = Keyboard.GetState();
            World.Query(_queryDescription, _updateControl);
            World.Query(_queryDescription, _setPrevStates);
            base.Update(t);
        }
    }
}
