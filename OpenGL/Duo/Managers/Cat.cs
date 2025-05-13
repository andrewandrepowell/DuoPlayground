using Duo.Data;
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
        private static readonly Keys[] _controlKeys = [Keys.Left, Keys.Right];
        private static readonly ReadOnlyDictionary<Actions, Animations> _actionAnimationMap = new(new Dictionary<Actions, Animations>()
        {
            { Actions.Idle, Animations.CatIdle },
            { Actions.Walk, Animations.CatWalk },
        });
        protected override IReadOnlyDictionary<Actions, Animations> ActionAnimationMap => _actionAnimationMap;
        public IList<Keys> ControlKeys => _controlKeys;
        public void UpdateControl(ButtonStates buttonState, Keys key)
        {
            if (buttonState == ButtonStates.Pressed && key == Keys.Left)
                MoveLeft();
            if (buttonState == ButtonStates.Released && key == Keys.Left)
                ReleaseLeft();
            if (buttonState == ButtonStates.Pressed && key == Keys.Right)
                MoveRight();
            if (buttonState == ButtonStates.Released && key == Keys.Right)
                ReleaseRight();
        }
        public override void Initialize(Map.PolygonNode node)
        {
            base.Initialize(node);
            Entity.Get<ControlComponent>().Manager.Initialize(this);
        }
    }
}
