using Arch.Core.Extensions;
using Duo.Data;
using Duo.Utilities;
using DuoGum.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Pow.Components;
using Pow.Utilities;
using Pow.Utilities.Animations;
using Pow.Utilities.Gum;
using Pow.Utilities.UA;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class UIIcon : Environment
    {
        private const Layers _layer = Layers.InterfaceComponent;
        private const PositionModes _positionMode = PositionModes.Screen;
        private AnimationManager _animationManager;
        private GumManager _gumManager;
        private uiTextView _uiTextView;
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            {
                _animationManager = Entity.Get<AnimationComponent>().Manager;
                _animationManager.Layer = _layer;
                _animationManager.PositionMode = _positionMode;
                _animationManager.Play((int)Enum.Parse<Animations>(node.Parameters["AnimationId"]));
            }
            { 
                _uiTextView = new();
                _gumManager = Entity.Get<GumComponent>().Manager;
                _gumManager.Initialize(_uiTextView.Visual);
                _gumManager.Layer = _layer;
                _gumManager.PositionMode = _positionMode;
                _gumManager.Origin = new(x: -16, y: _gumManager.Size.Height / 2);
            }
        }
        public Vector2 Position 
        { 
            get => _animationManager.Position; 
            set 
            {
                _animationManager.Position = value;
                _gumManager.Position = value;
            }
        }
        public float Rotation 
        { 
            get => _animationManager.Rotation;
            set
            {
                _animationManager.Rotation = value;
                _gumManager.Rotation = value;
            }
        }
        public bool Show 
        { 
            get => _animationManager.Show;
            set 
            { 
                _animationManager.Show = value;
                _gumManager.Show = value;
            } 
        }
        public string Text
        {
            get => _uiTextView.Text;
            set => _uiTextView.Text = value;
        }
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
        private AnimationManager _animationManager;
        private AnimationGroupManager _animationGroupManager;
        private UIGuide.Node _uiGuideNode;
        private UIIcon _pineConeUiIcon;
        private UIIcon _clockConeUiIcon;
        private Timer _timer;
        private int _pinecones;
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
            if (!_initialized) return;
            var frameNode = _uiGuideNode.FrameNodes[_animationManager.Index];
            if (frameNode.TopLeft.HasValue)
            {
                _pineConeUiIcon.Position = _animationManager.Position - _animationManager.Origin + frameNode.TopLeft.Value;
                _pineConeUiIcon.Rotation = frameNode.Rotation.Value;
                _pineConeUiIcon.Show = true;
            }
            else
            {
                _pineConeUiIcon.Show = false;
            }
            if (frameNode.BottomLeft.HasValue)
            {
                _clockConeUiIcon.Position = _animationManager.Position - _animationManager.Origin + frameNode.BottomLeft.Value;
                _clockConeUiIcon.Rotation = frameNode.Rotation.Value;
                _clockConeUiIcon.Show = true;
            }
            else
            {
                _clockConeUiIcon.Show = false;
            }
        }
        private void ServiceUpdatedTime()
        {
            if (!_initialized) return;
            _clockConeUiIcon.Text = _timer.CurrentTextTime;
        }
        private void UpdatePineconeUiText()
        {
            if (!_initialized) return;
            _pineConeUiIcon.Text = $"{_pinecones}";
        }
        public enum Actions { Opening, Idle, Twitching }
        public enum AnimationGroups { Opening, Idle, Twitching }
        public Actions Action => _action;
        public int Pinecones
        {
            get => _pinecones;
            set
            {
                if (_pinecones == value) return;
                _pinecones = value;
                UpdatePineconeUiText();
            }
        }
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            _initialized = false;
            var gameWindowSize = Globals.GameWindowSize;
            _animationManager = Entity.Get<AnimationComponent>().Manager;
            {
                _animationManager.Layer = _layer;
                _animationManager.PositionMode = _positionMode;
            }
            {
                _animationGroupManager = new(_animationManager);
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
                    })));
                Globals.DuoRunner.AddEnvironment(new(
                    Position: Vector2.Zero,
                    Vertices: null,
                    Parameters: new(new Dictionary<string, string>()
                    {
                        {"EntityType", "UIIcon"},
                        {"ID", "clock"},
                        {"AnimationId", "Clock"},
                    })));
            }
            {
                _timer = new();
                _timer.ServiceUpdatedTime = ServiceUpdatedTime;
            }
            {
                _pinecones = 0;
            }
            UpdateAction(Actions.Opening);
            {
                _animationManager.Position = new(
                    // x: gameWindowSize.Width / 2,
                    x: _size.Width / 2,
                    y: _size.Height / 2);
                _animationManager.ServiceFrameUpdated = ServiceFrameUpdated;
            }
        }
        public override void Cleanup()
        {
            base.Cleanup();
            Globals.DuoRunner.RemoveEnvironment(_pineConeUiIcon);
            Globals.DuoRunner.RemoveEnvironment(_clockConeUiIcon);
        }
        public void Twitch()
        {
            Debug.Assert(_action != Actions.Opening);
            UpdateAction(Actions.Twitching);
        }
        public override void Update()
        {
            base.Update();
            if (Pow.Globals.GamePaused) return;
            if (_pineConeUiIcon == null && _clockConeUiIcon == null)
            {
                Debug.Assert(!_initialized);
                var uiIcons = Globals.DuoRunner.Environments.OfType<UIIcon>().ToArray();
                if (uiIcons.Length == 2)
                {
                    _pineConeUiIcon = uiIcons.Where(x => x.ID == "pinecone").First();
                    _clockConeUiIcon = uiIcons.Where(x => x.ID == "clock").First();
                    _initialized = true;
                    ServiceUpdatedTime();
                    ServiceFrameUpdated();
                    UpdatePineconeUiText();
                }
            }
            if ((_action == Actions.Opening || _action == Actions.Twitching) && !_animationGroupManager.Running)
                UpdateAction(Actions.Idle);
            if (_action != Actions.Opening)
                _timer.Update();
        }
    }
}
