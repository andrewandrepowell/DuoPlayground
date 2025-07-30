using Duo.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class Collectible : Interactable
    {
        protected override IReadOnlyDictionary<Actions, int> ActionAnimationGroupMap => throw new NotImplementedException();
        protected override Boxes Boxes => Boxes.Collectible;
    }
}
