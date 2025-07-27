using Duo.Data;
using Pow.Utilities.Animations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class Door : Interactable
    {
        private enum AnimationGroups { Idle, Death }
        private static readonly ReadOnlyDictionary<Actions, int> _actionAnimationGroupMap = new(new Dictionary<Actions, int>()
        {
            { Actions.Waiting, (int)AnimationGroups.Idle },
            { Actions.Interacting, (int)AnimationGroups.Death }
        });
        protected override IReadOnlyDictionary<Actions, int> ActionAnimationGroupMap => _actionAnimationGroupMap;
        protected override Boxes Boxes => Boxes.RootBlockage;
        protected override bool DirectlyInteractable => false;
        protected override void Initialize(AnimationGroupManager manager)
        {
            base.Initialize(manager);
            manager.Configure(
                groupId: (int)AnimationGroups.Idle,
                group: new PlayIdleGroup(
                    primaryAnimationId: (int)Animations.RootBlockageIdle,
                    minWait: 2,
                    maxWait: 4,
                    secondaryAnimationIds: [(int)Animations.RootBlockageTwitch]));
            manager.Configure(
                groupId: (int)AnimationGroups.Death,
                group: new PlaySingleGroup((int)Animations.RootBlockageDeath));
        }
    }
}
