using Duo.Data;
using Microsoft.Xna.Framework.Input;
using Pow.Components;
using Pow.Utilities;
using Pow.Utilities.Control;
using Pow.Utilities.UA;
using System;
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
        });
        protected override IReadOnlyDictionary<Actions, Animations> ActionAnimationMap => _actionAnimationMap;
        protected override Boxes Boxes => Boxes.Cat;
        public Keys[] ControlKeys => _uaManager.ControlKeys;
        public void UpdateControl(ButtonStates buttonState, Keys key) => _uaManager.UpdateControl(buttonState, key);
        public void UpdateUserAction(int actionId, ButtonStates buttonState)
        {
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
                MoveLeft();
            }

            if (released && right && MovingRight)
                ReleaseRight();
            if (pressed && right)
            {
                if (MovingLeft)
                    ReleaseLeft();
                MoveRight();
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
