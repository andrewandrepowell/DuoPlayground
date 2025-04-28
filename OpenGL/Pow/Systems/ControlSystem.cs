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
        private readonly ForEach _updateControl;
        private KeyboardState _keyboardState;
        private void UpdateControl(in Entity entity)
        {
            var controlManager = World.Get<ControlComponent>(entity).Manager;
            foreach (var key in controlManager.Control.ControlKeys)
                if (_keyboardState.IsKeyDown(key))
                    controlManager.Control.UpdateControl(
                        input: Inputs.Keyboard,
                        buttonState: ButtonStates.Pressed,
                        key: key);
                else if (_keyboardState.IsKeyDown(key))
                    controlManager.Control.UpdateControl(
                        input: Inputs.Keyboard,
                        buttonState: ButtonStates.Pressed,
                        key: key);
        }
        public ControlSystem(World world) : base(world)
        {
            _updateControl = new((Entity entity) => UpdateControl(in entity));
        }
        public override void Update(in GameTime t)
        {
            _keyboardState = Keyboard.GetState();
            World.Query(_queryDescription, _updateControl);
            base.Update(t);
        }
    }
}
