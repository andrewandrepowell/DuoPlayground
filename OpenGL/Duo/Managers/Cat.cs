using Arch.Core.Extensions;
using Duo.Data;
using Duo.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using nkast.Aether.Physics2D.Dynamics;
using Pow.Components;
using Pow.Utilities;
using Pow.Utilities.Control;
using Pow.Utilities.UA;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class Cat : Character, IUserAction, IControl
    {
        private bool _initialized;
        private UAManager _uaManager;
        private UI _ui;
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
        protected override Layers Layer => Layers.Protag;
        protected override Utilities.Physics.Character.ServiceInteractableContact ServiceInteractableContact => ContactInteractable;
        public Keys[] ControlKeys => _uaManager.ControlKeys;
        public Buttons[] ControlButtons => _uaManager.ControlButtons;
        public Directions[] ControlThumbsticks => _uaManager.ControlThumbsticks;
        public void UpdateControl(ButtonStates buttonState, Keys key) => _uaManager.UpdateControl(buttonState, key);
        public void UpdateControl(ButtonStates buttonState, Buttons button) => _uaManager.UpdateControl(buttonState, button);
        public void UpdateControl(Directions thumbsticks, Vector2 position) => _uaManager.UpdateControl(thumbsticks, position);
        public void UpdateUserAction(int actionId, ButtonStates buttonState, float strength)
        {
            if (Pow.Globals.GamePaused || !_initialized) 
                return;

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
        private void ContactInteractable(in Utilities.Physics.Character.BoxNode boxNode, Interactable interactable)
        {
            if (Pow.Globals.GamePaused || !_initialized)
                return;

            if (boxNode.BoxType == BoxTypes.Ground && 
                interactable is Key key && 
                key.Action == Interactable.Actions.Waiting)
                key.Interact();
            if (boxNode.BoxType == BoxTypes.Collide &&
                interactable is Collectible collectible &&
                collectible.Action == Interactable.Actions.Waiting)
            {
                collectible.Interact();
                if (collectible.Mode == Collectible.Modes.PineCone)
                {
                    _ui.Pinecones += 1;
                }
                if (_ui.Action != UI.Actions.Opening)
                { 
                    _ui.Twitch();
                }
            }
        }
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            Entity.Get<ControlComponent>().Manager.Initialize(this);
            _uaManager = Globals.DuoRunner.UAGenerator.Acquire();
            _uaManager.Initialize(this);
            _ui = null;
            _initialized = false;
        }
        public override void Update()
        {
            base.Update();

            // Initialize based on finding related in game environments.
            {
                if (_ui == null)
                {
                    var uis = Globals.DuoRunner.Environments.OfType<UI>().ToArray();
                    Debug.Assert(uis.Length == 1);
                    _ui = uis[0];
                }
                if (!_initialized && _ui != null)
                {
                    _initialized = true;
                }
            }
            Debug.Assert(_initialized);
        }
    }
}
