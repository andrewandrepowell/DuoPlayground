﻿using Duo.Data;
using Microsoft.Xna.Framework.Input;
using Pow.Components;
using Pow.Utilities;
using Pow.Utilities.Control;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Arch.Core.Extensions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class Cat : Character, IControl
    {
        private static readonly Keys[] _controlKeys = [Keys.Left, Keys.Right, Keys.Space];
        private static readonly ReadOnlyDictionary<Actions, Animations> _actionAnimationMap = new(new Dictionary<Actions, Animations>()
        {
            { Actions.Idle, Animations.CatIdle },
            { Actions.Walk, Animations.CatWalk },
        });
        protected override IReadOnlyDictionary<Actions, Animations> ActionAnimationMap => _actionAnimationMap;
        protected override Boxes Boxes => Boxes.Cat;
        public Keys[] ControlKeys => _controlKeys;
        public void UpdateControl(ButtonStates buttonState, Keys key)
        {
            if (buttonState == ButtonStates.Released && key == Keys.Left && MovingLeft)
                ReleaseLeft();
            if (buttonState == ButtonStates.Pressed && key == Keys.Left)
            {
                if (MovingRight)
                    ReleaseRight();
                MoveLeft();
            }
            
            if (buttonState == ButtonStates.Released && key == Keys.Right && MovingRight)
                ReleaseRight();
            if (buttonState == ButtonStates.Pressed && key == Keys.Right)
            {
                if (MovingLeft)
                    ReleaseLeft();
                MoveRight();
            }

            if (buttonState == ButtonStates.Released && key == Keys.Space && Jumping)
                ReleaseJump();
            if (buttonState == ButtonStates.Pressed && key == Keys.Space && Grounded && !Jumping)
                Jump();
        }
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            Entity.Get<ControlComponent>().Manager.Initialize(this);
        }
    }
}
