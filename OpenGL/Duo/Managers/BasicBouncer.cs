using Duo.Data;
using Pow.Utilities;
using Pow.Utilities.Animations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Duo.Managers;

internal class BasicBouncer : Bouncer
{
    
    private enum AnimationGroups { Idle, Bounce }
    private static readonly ReadOnlyDictionary<Actions, int> _actionAnimationGroupMap = new(new Dictionary<Actions, int>()
    {
        { Actions.Waiting, (int)AnimationGroups.Idle },
        { Actions.Bouncing, (int)AnimationGroups.Bounce }
    });
    private Boxes _boxes;
    private Layers _layer;
    private Dictionary<Actions, Animations> _actionAnimationMap;
    protected override IReadOnlyDictionary<Actions, int> ActionAnimationGroupMap => _actionAnimationGroupMap;
    protected override Boxes Boxes => _boxes;
    protected override Layers Layer => _layer;
    protected override void Initialize(AnimationGroupManager manager)
    {
        manager.Configure(
            groupId: _actionAnimationGroupMap[Actions.Waiting],
            group: new PlaySingleGroup((int)_actionAnimationMap[Actions.Waiting]));
        manager.Configure(
            groupId: _actionAnimationGroupMap[Actions.Bouncing],
            group: new PlaySingleGroup((int)_actionAnimationMap[Actions.Bouncing]));
    }
    public override void Initialize(PolygonNode node)
    {
        _boxes = Enum.Parse<Boxes>(node.Parameters.GetValueOrDefault("Boxes", "BasicBouncer"));
        _layer = Enum.Parse<Layers>(node.Parameters.GetValueOrDefault("Layer", "Ground"));
        _actionAnimationMap = new()
        {
            { Actions.Waiting, Enum.Parse<Animations>(node.Parameters.GetValueOrDefault("WaitAnimation", "BasicBouncerIdle")) },
            { Actions.Bouncing, Enum.Parse<Animations>(node.Parameters.GetValueOrDefault("BounceAnimation", "BasicBouncerBounce")) }
        };
        base.Initialize(node);
    }
}