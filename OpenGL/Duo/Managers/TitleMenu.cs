using Arch.Core.Extensions;
using Duo.Data;
using Duo.Utilities.Shaders;
using DuoGum.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Screens.Transitions;
using Pow.Components;
using Pow.Utilities;
using Pow.Utilities.Animations;
using Pow.Utilities.ParticleEffects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class TitleMenuButton : Environment
    {
        private static readonly Layers _layer = Layers.MidSky;
        private const PositionModes _positionMode = PositionModes.Screen;
        private AnimationManager _animationManager;
        private TitleMenuGlowButtonFeature _buttonFeature;
        private bool _selected;
        public Vector2 Position
        {
            get => _animationManager.Position;
            set => _animationManager.Position = value;
        }
        public float Rotation
        {
            get => _animationManager.Rotation;
            set => _animationManager.Rotation = value;
        }
        public float Visibility
        {
            get => _animationManager.Visibility;
            set => _animationManager.Visibility = value;
        }
        public bool Selected
        {
            get => _selected;
            set
            {
                if (_selected == value) return;
                _selected = value;
                if (_selected)
                {
                    _buttonFeature.GlowIntensity = 1.0f;
                }
                else
                {
                    _buttonFeature.GlowIntensity = 0.0f;
                }
            }
        }
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            _animationManager = Entity.Get<AnimationComponent>().Manager;
            _animationManager.Pauseable = false;
            _animationManager.Layer = _layer;
            _animationManager.PositionMode = _positionMode;
            _animationManager.Play((int)Animations.TitleMenuButton);
            _animationManager.Show = true;
            _buttonFeature = _animationManager.CreateFeature<TitleMenuGlowButtonFeature, TitleMenuGlowButtonEffect>();
            _buttonFeature.Layer = _layer;
            _buttonFeature.GlowIntensity = 0.0f;
            _selected = false;
        }
    }
    internal class TitleMenu : GumObject
    {
        private bool _initialized;
        private titleView _view;
        private ReadOnlyDictionary<string, TitleMenuButton> _buttons;
        private string _dimmerID;
        private Dimmer _dimmer;
        private string _transitionID;
        private Image _transition;
        private RunningStates _state;
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            {
                _view = new titleView();
                GumManager.Initialize(_view.Visual);
                GumManager.Position = GumManager.Origin;
                GumManager.Layer = Layers.MidSky;
                GumManager.PositionMode = PositionModes.Screen;
            }
            {
                foreach (var button in _view.buttons.Buttons)
                {
                    Globals.DuoRunner.AddEnvironment(new(
                        Position: Vector2.Zero,
                        Vertices: null,
                        Parameters: new(new Dictionary<string, string>()
                        {
                            {"EntityType", "TitleMenuButton"},
                            {"ID", $"t_btn_{button.Message}"},
                        })));
                }
                _buttons = null;
            }
            {
                _dimmer = null;
                _dimmerID = node.Parameters.GetValueOrDefault("DimmerID", "Dimmer");
            }
            {
                _transition = null;
                _transitionID = node.Parameters.GetValueOrDefault("TransitionID", "Transition");
            }
            {
                _state = RunningStates.Waiting;
                _initialized = false;
            }
        }
        public override void Cleanup()
        {
            Debug.Assert(_initialized);
            _initialized = false;
            foreach (var button in _buttons.Values)
            {
                var duoRunner = Globals.DuoRunner;
                duoRunner.RemoveEnvironment(button);
            }
            base.Cleanup();
        }
        public override void Update()
        {
            base.Update();

            if (_dimmer == null)
            {
                _dimmer = Globals.DuoRunner.Environments.OfType<Dimmer>().Where(dimmer => dimmer.ID == _dimmerID).First();
                Debug.Assert(_dimmer.State == RunningStates.Running);
            }

            if (_transition == null)
            {
                _transition = Globals.DuoRunner.Environments.OfType<Image>().Where(i => i.ID == _transitionID).First();
                //Debug.Assert(!_transition.Running);
            }
           
            if (_buttons == null)
            {
                var ids = _view.buttons.Buttons.Select(x => x.Message).ToArray();
                var titleMenuButtons = Globals.DuoRunner.Environments.OfType<TitleMenuButton>().ToArray();
                if (titleMenuButtons.Length == _view.buttons.Buttons.Length)
                {
                    // Associate button id with title menu button.
                    var buttons = new Dictionary<string, TitleMenuButton>();
                    foreach (var id in ids)
                    {
                        var titleMenuButton = titleMenuButtons.Where(x => x.ID == $"t_btn_{id}").First();
                        buttons.Add(id, titleMenuButton);
                    }
                    Debug.Assert(buttons.Keys.Count == _view.buttons.Buttons.Length);
                    _buttons = new(buttons);

                    // Initialize each title menu button.
                    foreach (var gumButton in _view.buttons.Buttons)
                    {
                        var button = _buttons[gumButton.Message];
                        var position = new Vector2(x: gumButton.Visual.AbsoluteX, y: gumButton.Visual.AbsoluteY + gumButton.Visual.Height / 2);
                        button.Position = position;
                        button.Rotation = -gumButton.buttonContainerRotation / 180.0f * MathHelper.Pi;
                    }
                }
            }

#if DEBUG
            if (!_initialized)
            {
                var titleMenus = Globals.DuoRunner.Environments.OfType<TitleMenu>().ToArray();
                Debug.Assert(titleMenus.Length == 1);
            }
#endif
            // Initialize when all relevant components are initialized,
            if (!_initialized && _dimmer != null && _buttons != null && _transition != null && !_transition.Running)
            {
                _initialized = true;
                Open();
            }
            _debugWait -= Pow.Globals.GameTime.GetElapsedSeconds();

            // If running (i.e. not dimming or brightening) then make sure the focused button is selected,
            if (_initialized && _state == RunningStates.Running)
                foreach (var gumButton in _view.buttons.Buttons)
                    _buttons[gumButton.Message].Selected = gumButton.IsFocused;

            // Update state once dimmer component is finished its operation.
            if (_state == RunningStates.Starting && _dimmer.State == RunningStates.Waiting && !_transition.Running)
                ForceOpen();
            else if (_state == RunningStates.Stopping && _dimmer.State == RunningStates.Running && !_transition.Running)
                ForceClose();
        }
        public void Open()
        {
            Debug.Assert(_initialized);
            Debug.Assert(_state == RunningStates.Waiting);
            Debug.Assert(_dimmer.State == RunningStates.Running);
            Debug.Assert(!_transition.Running && _transition.Animation == Animations.TransitionRunning);
            Debug.Assert(!_view.buttons.ButtonFocused);
            _dimmer.Stop();
            _transition.Play(Animations.TransitionStopping);
            _transition.Visibility = 1;
            _state = RunningStates.Starting;
        }
        public void Close()
        {
            Debug.Assert(_initialized);
            Debug.Assert(_state == RunningStates.Running);
            Debug.Assert(_dimmer.State == RunningStates.Waiting);
            Debug.Assert(!_transition.Running && _transition.Animation == Animations.TransitionWaiting);
            Debug.Assert(_view.buttons.ButtonFocused);
            var menu = _view.buttons;
            menu.ResetFocus();
            _dimmer.Start();
            _transition.Play(Animations.TransitionStarting);
            _transition.Visibility = 1;
            _state = RunningStates.Stopping;
        }
        private void ForceOpen()
        {
            var menu = _view.buttons;
            menu.ResetFocus();
            menu.start.IsFocused = true;
            _dimmer.ForceStop();
            _transition.Play(Animations.TransitionWaiting);
            _transition.Visibility = 0;
            _state = RunningStates.Running;
        }
        private void ForceClose()
        {
            Pow.Globals.GameResume();
            var menu = _view.buttons;
            menu.ResetFocus();
            _dimmer.ForceStart();
            _transition.Play(Animations.TransitionRunning);
            _transition.Visibility = 1;
            _state = RunningStates.Waiting;
        }
    }
}
