using Duo.Data;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal partial class Character
    {
        protected abstract IReadOnlyDictionary<Actions, Animations> ActionAnimationMap { get; }
        protected abstract Boxes Boxes { get; }
        protected virtual Layers Layer => Layers.Ground;
        protected virtual bool Flying => false;
        protected virtual Utilities.Physics.Character.ServiceInteractableContact ServiceInteractableContact => null;
    }
}
