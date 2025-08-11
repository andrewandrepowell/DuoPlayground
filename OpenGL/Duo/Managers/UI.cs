using Arch.Core.Extensions;
using Duo.Data;
using Duo.Utilities;
using Pow.Components;
using Pow.Utilities;
using Pow.Utilities.Animations;
using Pow.Utilities.UA;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Duo.Managers
{
    internal class UI : Environment
    {
        private const Layers _layer = Layers.Interface;
        private const PositionModes _positionMode = PositionModes.Screen;
        private const Masks _mask = Masks.UIGuide;
        private readonly static Color _topLeftColorUIGuide = new(0xff_30_be_6a);
        private readonly static Color _topRightColorUIGuide = new(0xff_36_f2_fb);
        private readonly static Color _bottomLeftColorUIGuide = new(0xff_26_71_df);
        private readonly static SizeF _size = new(192, 128);
        private readonly static ReadOnlyDictionary<Actions, AnimationGroups> _actionAnimationGroupMap = new(new Dictionary<Actions, AnimationGroups>() 
        {
            { Actions.Opening, AnimationGroups.Opening },
            { Actions.Idle, AnimationGroups.Idle },
            { Actions.Twitching, AnimationGroups.Twitching },   
        });
        private Actions _action;
        private AnimationGroupManager _animationGroupManager;
        private UIGuide.Node _uiGuideNode;
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
            {
                var maskNode = Globals.DuoRunner.MaskGenerator.GetNode((int)_mask);
                _uiGuideNode = UIGuide.GetNode(
                    maskNode: maskNode,
                    direction: Directions.Left,
                    topLeftColor: _topLeftColorUIGuide,
                    topRightColor: _topRightColorUIGuide,
                    bottomLeftColor: _bottomLeftColorUIGuide);
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
