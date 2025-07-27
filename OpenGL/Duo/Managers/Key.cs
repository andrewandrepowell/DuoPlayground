using Duo.Data;
using Pow.Utilities;
using Pow.Utilities.Animations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class Key : Interactable
    {
        private bool _initialized = false;
        private Door[] _doors;
        private enum AnimationGroups { Idle, Death }
        private static readonly ReadOnlyDictionary<Actions, int> _actionAnimationGroupMap = new(new Dictionary<Actions, int>()
        {
            { Actions.Waiting, (int)AnimationGroups.Idle },
            { Actions.Interacting, (int)AnimationGroups.Death }
        });
        protected override IReadOnlyDictionary<Actions, int> ActionAnimationGroupMap => _actionAnimationGroupMap;
        protected override Boxes Boxes => Boxes.Root;
        protected override void Initialize(AnimationGroupManager manager)
        {
            base.Initialize(manager);
            manager.Configure(
                groupId: (int)AnimationGroups.Idle,
                group: new PlayIdleGroup(
                    primaryAnimationId: (int)Animations.TreeRootIdle,
                    minWait: 2,
                    maxWait: 4,
                    secondaryAnimationIds: [(int)Animations.TreeRootTwitch]));
            manager.Configure(
                groupId: (int)AnimationGroups.Death,
                group: new PlaySingleGroup((int)Animations.TreeRootDeath));
        }
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            Debug.Assert(!_initialized);
            _doors = null;
        }
        public override void Cleanup()
        {
            Debug.Assert(_initialized);
            _initialized = false;
            base.Cleanup();
        }
        public override void Update()
        {
            base.Update();
            if (Pow.Globals.GamePaused)
                return;
            if (!_initialized)
            {
                Debug.Assert(ID != null);
                _doors = Globals.DuoRunner.Environments.OfType<Door>().Where(x => x.ID == ID).ToArray();
                _initialized = true;
            }
        }
        protected override void FinishInteracting()
        {
            base.FinishInteracting();
            Debug.Assert(_initialized);
            foreach (ref var door in _doors.AsSpan())
                if (door.Action == Actions.Waiting)
                    door.Interact();
        }
    }
}
