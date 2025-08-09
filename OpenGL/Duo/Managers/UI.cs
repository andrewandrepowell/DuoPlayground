using Arch.Core.Extensions;
using Duo.Data;
using Pow.Components;
using Pow.Utilities;
using Pow.Utilities.Animations;
using Pow.Utilities.UA;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class UI : Environment
    {
        private const Layers _layer = Layers.Interface;
        private const PositionModes _positionMode = PositionModes.Screen;
        private readonly static SizeF _size = new(192, 128);
        private readonly static ReadOnlyDictionary<Actions, AnimationGroups> _actionAnimationGroupMap = new(new Dictionary<Actions, AnimationGroups>() 
        {
            { Actions.Opening, AnimationGroups.Opening },
            { Actions.Idle, AnimationGroups.Idle },
            { Actions.Twitching, AnimationGroups.Twitching },   
        });
        private Actions _action;
        private AnimationGroupManager _animationGroupManager;
        private void UpdateAction(Actions action)
        {
            if (_actionAnimationGroupMap.TryGetValue(action, out var groupId))
            {
                _animationGroupManager.Play((int)groupId);
            }
            _action = action;
        }
        public enum Actions { Opening, Idle, Twitching }
        public enum AnimationGroups { Opening, Idle, Twitching }
        public Actions Action => _action;
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            var gameWindowSize = Globals.GameWindowSize;
            var animationManager = Entity.Get<AnimationComponent>().Manager;
            {
                animationManager.Layer = _layer;
                animationManager.PositionMode = _positionMode;
            }
            {
                _animationGroupManager = new(animationManager);
                _animationGroupManager.Configure(
                    groupId: (int)AnimationGroups.Opening,
                    group: new PlaySingleGroup((int)Animations.UIOpening));
                _animationGroupManager.Configure(
                    groupId: (int)AnimationGroups.Idle,
                    group: new PlaySingleGroup((int)Animations.UIIdle));
                _animationGroupManager.Configure(
                    groupId: (int)AnimationGroups.Twitching,
                    group: new PlaySingleGroup((int)Animations.UITwitch));
                _animationGroupManager.Initialize();
            }
            UpdateAction(Actions.Opening);
            {
                animationManager.Position = new(
                    x: gameWindowSize.Width / 2,
                    y: _size.Height / 2);
            }
        }
        public void Twitch()
        {
            Debug.Assert(_action != Actions.Opening);
            UpdateAction(Actions.Twitching);
        }
        public override void Update()
        {
            base.Update();
            if ((_action == Actions.Opening || _action == Actions.Twitching) && !_animationGroupManager.Running)
                UpdateAction(Actions.Idle);

        }
    }
}
