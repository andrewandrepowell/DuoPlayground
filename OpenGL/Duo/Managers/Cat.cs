using Duo.Data;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Pow.Components;
using Pow.Utilities;
using Pow.Utilities.Control;
using Pow.Utilities.UA;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Arch.Core.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duo.Utilities;

namespace Duo.Managers
{
    internal class Cat : Character, IUserAction, IControl
    {
        private UAManager _uaManager;
        private static readonly ReadOnlyDictionary<Actions, Animations> _actionAnimationMap = new(new Dictionary<Actions, Animations>()
        {
            { Actions.Idle, Animations.CatIdle },
            { Actions.Walk, Animations.CatWalk },
            { Actions.Jump, Animations.CatJump },
            { Actions.Land, Animations.CatLand },
            { Actions.Fall, Animations.CatFall },
        });
        protected override IReadOnlyDictionary<Actions, Animations> ActionAnimationMap => _actionAnimationMap;
        protected override Boxes Boxes => Boxes.Cat;
        public Keys[] ControlKeys => _uaManager.ControlKeys;
        public Buttons[] ControlButtons => _uaManager.ControlButtons;
        public Directions[] ControlThumbsticks => _uaManager.ControlThumbsticks;
        public void UpdateControl(ButtonStates buttonState, Keys key) => _uaManager.UpdateControl(buttonState, key);
        public void UpdateControl(ButtonStates buttonState, Buttons button) => _uaManager.UpdateControl(buttonState, button);
        public void UpdateControl(Directions thumbsticks, Vector2 position) => _uaManager.UpdateControl(thumbsticks, position);
        public void UpdateUserAction(int actionId, ButtonStates buttonState, float strength)
        {
            if (Pow.Globals.GamePaused) return;

            Debug.Print($"Strength: {strength}");

            var control = (Controls)actionId;
            var left = control == Controls.MoveLeft;
            var right = control == Controls.MoveRight;
            var jump = control == Controls.Jump;
            var released = buttonState == ButtonStates.Released;
            var pressed = buttonState == ButtonStates.Pressed;

            if (released && left && MovingLeft)
                ReleaseLeft();
            if (pressed && left)
            {
                if (MovingRight)
                    ReleaseRight();
                if (MovingLeft)
                    UpdateLeft(strength);
                else
                    MoveLeft(strength);
            }

            if (released && right && MovingRight)
                ReleaseRight();
            if (pressed && right)
            {
                if (MovingLeft)
                    ReleaseLeft();
                if (MovingRight) 
                    UpdateRight(strength);
                else
                    MoveRight(strength);
            }

            if (released && jump && Jumping)
                ReleaseJump();
            if (pressed && jump && Grounded && !Jumping)
                Jump();
        }
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            Entity.Get<ControlComponent>().Manager.Initialize(this);
            _uaManager = Globals.DuoRunner.UAGenerator.Acquire();
            _uaManager.Initialize(this);
        }
    }
}
