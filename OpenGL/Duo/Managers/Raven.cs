using Duo.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers;

internal class Raven : NPC
{
    protected override IReadOnlyDictionary<Actions, Animations> ActionAnimationMap => throw new NotImplementedException();

    protected override Boxes Boxes => throw new NotImplementedException();
}
