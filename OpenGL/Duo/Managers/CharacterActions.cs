using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duo.Data;

namespace Duo.Managers
{
    internal partial class Character
    {
        private Actions _action = Actions.Idle;
        private void UpdateAction(Actions action)
        {
            if (ActionAnimationMap.TryGetValue(action, out var animation))
            {
                var animationManager = AnimationManager;
                animationManager.Play((int)animation);
            }
            _action = action;
        }
        private void InitializeAction()
        {
            UpdateAction(Actions.Idle);
        }

        public enum Actions { Walk, Idle }
        protected abstract IReadOnlyDictionary<Actions, Animations> ActionAnimationMap { get; }
    }
}
