using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duo.Data;

namespace Duo.Managers
{
    internal partial class Character
    {
        private static readonly Actions[] _flyingActions = [Actions.Idle, Actions.Walk];
        private Actions _action = Actions.Idle;
        private void UpdateAction(Actions action)
        {
#if DEBUG
            if (Flying)
                Debug.Assert(_flyingActions.Contains(action));
#endif
            if (ActionAnimationMap.TryGetValue(action, out var animation))
            {
                var animationManager = AnimationManager;
                animationManager.Play((int)animation);
            }
            _action = action;
        }
        private void InitializeAction()
        {
            {
                var animationManager = AnimationManager;
                animationManager.Layer = Layer;
            }
            UpdateAction(Actions.Idle);
        }
        public enum Actions { Walk, Idle, Jump, Fall, Land, Express }
        public Actions Action => _action;
    }
}
