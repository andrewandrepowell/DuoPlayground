using Duo.Data;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class Cat : Character
    {
        private static readonly ReadOnlyDictionary<Actions, Animations> _actionAnimationMap = new(new Dictionary<Actions, Animations>()
        {
            { Actions.Idle, Animations.CatIdle },
            { Actions.Walk, Animations.CatWalk },
        });
        protected override IReadOnlyDictionary<Actions, Animations> ActionAnimationMap => _actionAnimationMap;
        public override void Initialize(Map.PolygonNode node)
        {
            base.Initialize(node);

        }
    }
}
