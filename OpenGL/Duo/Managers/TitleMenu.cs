using Arch.Core.Extensions;
using Duo.Data;
using Duo.Utilities.Shaders;
using DuoGum.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Screens.Transitions;
using MonoGameGum.Forms.Controls;
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
            _buttonFeature.GlowColor = Color.Black;
            _selected = false;
        }
    }
    internal class TitleMenu : GumObject
    {
        private bool _initialized;
        private titleView _view;
        private ReadOnlyDictionary<string, TitleMenuButton> _buttons;
        private TransitionBranches _branches;
        private string _branchesID;
        private OptionsMenu _options;
        private string _optionsID;
        private RunningStates _state;
        private Action _nextAction;
#if DEBUG
        private float _debugWait;
#endif
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
#if DEBUG
            {
                _debugWait = 1;
            }
#endif
            {
                _view = new titleView();
                GumManager.Initialize(_view.Visual);
                GumManager.Position = GumManager.Origin;
                GumManager.Layer = Layers.MidSky;
                GumManager.PositionMode = PositionModes.Screen;
            }
            {
                _view.buttons.start.Click += (object? sender, EventArgs e) => Transition(Start);
                _view.buttons.options.Click += (object? sender, EventArgs e) => OpenOptions();
                _view.buttons.exit.Click += (object? sender, EventArgs e) => Transition(Pow.Globals.Game.Exit);
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
                _branches = null;
                _branchesID = node.Parameters.GetValueOrDefault("BranchesID", "TitleTransitionBranches");
            }
            {
                _options = null;
                _optionsID = node.Parameters.GetValueOrDefault("OptionsID", "Options");
            }
            {
                _nextAction = null;
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

            if (_options == null)
            {
                var options = Globals.DuoRunner.Environments.OfType<OptionsMenu>().Where(options => options.ID == _optionsID).First();
                if (options.Initialized)
                {
                    _options = options;
                    Debug.Assert(_options.State == RunningStates.Waiting);
                    _options.BackAction = CloseOptions;
                }
            }

            if (_branches == null)
            {
                var branches = Globals.DuoRunner.Environments.OfType<TransitionBranches>().Where(branches => branches.ID == _branchesID).First();
                if (branches.Initialized)
                {
                    _branches = branches;
                    Debug.Assert(_branches.State == RunningStates.Waiting);
                }
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
            if (!_initialized && _branches != null && _options != null)
            {
#if DEBUG
                if (_debugWait <= 0)
                {
#endif
                    _initialized = true;
                    Open();
#if DEBUG
                }
#endif
            }

#if DEBUG
            if (_debugWait > 0)
            {
                _debugWait -= Pow.Globals.GameTime.GetElapsedSeconds();
            }
#endif

            // If running (i.e. not dimming or brightening) then make sure the focused button is selected,
            if (_initialized && _state == RunningStates.Running && _options.State == RunningStates.Waiting)
                foreach (var gumButton in _view.buttons.Buttons)
                    _buttons[gumButton.Message].Selected = gumButton.IsFocused;

            // Update state once dimmer component is finished its operation.
            if (_state == RunningStates.Starting && _branches.State == RunningStates.Running)
                ForceOpen();
            else if (_state == RunningStates.Stopping && _branches.State == RunningStates.Waiting)
                ForceClose();
        }
        private void Open()
        {
            Debug.Assert(_initialized);
            Debug.Assert(_state == RunningStates.Waiting);
            Debug.Assert(_branches.State == RunningStates.Waiting);
            Debug.Assert(_options.State == RunningStates.Waiting);
            Debug.Assert(!_view.buttons.ButtonFocused);
            _branches.Open();
            _state = RunningStates.Starting;
        }
        private void Close()
        {
            Debug.Assert(_initialized);
            Debug.Assert(_state == RunningStates.Running);
            Debug.Assert(_branches.State == RunningStates.Running);
            Debug.Assert(_options.State == RunningStates.Waiting);
            Debug.Assert(_view.buttons.ButtonFocused);
            var menu = _view.buttons;
            menu.ResetFocus();
            _branches.Close();
            _state = RunningStates.Stopping;
        }
        private void ForceOpen()
        {
            var menu = _view.buttons;
            menu.ResetFocus();
            menu.start.IsFocused = true;
            _branches.ForceOpen();
            _state = RunningStates.Running;
        }
        private void ForceClose()
        {
            var menu = _view.buttons;
            menu.ResetFocus();
            _branches.ForceClose();
            _state = RunningStates.Waiting;
            _nextAction?.Invoke();
            _nextAction = null;
        }
        private void Transition(Action nextAction)
        {
            _nextAction = nextAction;
            Close();
        }
        private void Start()
        {
            Pow.Globals.Runner.Map.LoadNext((int)Maps.LevelDebug2);
        }
        private void OpenOptions()
        {
            Debug.Assert(_initialized);
            Debug.Assert(_state == RunningStates.Running);
            Debug.Assert(_branches.State == RunningStates.Running);
            Debug.Assert(_options.State == RunningStates.Waiting);
            Debug.Assert(_view.buttons.ButtonFocused);
            var menu = _view.buttons;
            menu.ResetFocus();
            _options.Open();
        }
        private void CloseOptions()
        {
            Debug.Assert(_initialized);
            Debug.Assert(_state == RunningStates.Running);
            Debug.Assert(_branches.State == RunningStates.Running);
            Debug.Assert(_options.State == RunningStates.Waiting);
            Debug.Assert(!_view.buttons.ButtonFocused);
            var menu = _view.buttons;
            menu.ResetFocus();
            menu.options.IsFocused = true;
        }
    }
}
