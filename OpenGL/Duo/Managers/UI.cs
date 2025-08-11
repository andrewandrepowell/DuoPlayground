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
    internal class UIIcon : Environment
    {
        private AnimationManager _animationManager;
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            _animationManager = Entity.Get<AnimationComponent>().Manager;
            _animationManager.Layer = Enum.Parse<Layers>(node.Parameters["Layer"]);
            _animationManager.PositionMode = Enum.Parse<PositionModes>(node.Parameters["PositionMode"]);
            _animationManager.Play((int)Enum.Parse<Animations>(node.Parameters["AnimationId"]));
        }
        public Vector2 Position { get => _animationManager.Position; set => _animationManager.Position = value; }
        public float Rotation { get => _animationManager.Rotation; set => _animationManager.Rotation = value; }
    }
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
        private bool _initialized = false;
        private Actions _action;
        private AnimationGroupManager _animationGroupManager;
        private UIGuide.Node _uiGuideNode;
        private UIIcon _pineConeUiIcon;
        private void UpdateAction(Actions action)
        {
            if (_actionAnimationGroupMap.TryGetValue(action, out var groupId))
            {
                _animationGroupManager.Play((int)groupId);
            }
            _action = action;
        }
        private void ServiceFrameUpdated()
        {

        }
        public enum Actions { Opening, Idle, Twitching }
        public enum AnimationGroups { Opening, Idle, Twitching }
        public Actions Action => _action;
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            _initialized = false;
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
            {
                Globals.DuoRunner.AddEnvironment(new(
                    Position: Vector2.Zero,
                    Vertices: null,
                    Parameters: new(new Dictionary<string, string>()
                    {
                        {"EntityType", "UIIcon"},
                        {"ID", "pinecone"},
                        {"AnimationId", "PineCone"},
                        {"Layer", Enum.GetName(_layer)},
                        {"PositionMode", Enum.GetName(_positionMode)}
                    })));
            }
            UpdateAction(Actions.Opening);
            {
                animationManager.Position = new(
                    x: gameWindowSize.Width / 2,
                    y: _size.Height / 2);
            }
        }
        public override void Cleanup()
        {
            base.Cleanup();
            Globals.DuoRunner.RemoveEnvironment(_pineConeUiIcon);
        }
        public void Twitch()
        {
            Debug.Assert(_action != Actions.Opening);
            UpdateAction(Actions.Twitching);
        }
        public override void Update()
        {
            base.Update();
            if (_pineConeUiIcon != null)
            {
                Debug.Assert(!_initialized);
                var uiIcons = Globals.DuoRunner.Environments.OfType<UIIcon>().ToArray();
                Debug.Assert(uiIcons.Length == 1);
                _initialized = true;
            }
            Debug.Assert(_initialized);
            if ((_action == Actions.Opening || _action == Actions.Twitching) && !_animationGroupManager.Running)
                UpdateAction(Actions.Idle);
        }
    }
}
