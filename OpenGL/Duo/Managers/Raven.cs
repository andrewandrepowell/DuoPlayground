using Duo.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers;

internal class Raven : NPC
{
    private static readonly ReadOnlyDictionary<Actions, Animations> _actionAnimationMap = new(new Dictionary<Actions, Animations>()
    {
        { Actions.Idle, Animations.RavenFly },
        { Actions.Walk, Animations.RavenFly },
    });
    protected override IReadOnlyDictionary<Actions, Animations> ActionAnimationMap => _actionAnimationMap;
    protected override Boxes Boxes => Boxes.Bird;
    protected override bool Flying => true;
}
