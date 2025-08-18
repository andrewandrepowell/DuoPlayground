using Arch.Core.Extensions;
using Duo.Data;
using DuoGum.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Pow.Components;
using Pow.Utilities;
using Pow.Utilities.Animations;
using Pow.Utilities.Control;
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
    internal class MainMenuButton : Environment
    {
        private static readonly ReadOnlyDictionary<Modes, Layers> _layers = new(new Dictionary<Modes, Layers>() 
        {
            { Modes.Background, Layers.Menu },
            { Modes.Foreground, Layers.MenuForeground },
        });
        private static readonly ReadOnlyDictionary<Modes, Animations> _animations = new(new Dictionary<Modes, Animations>()
        {
            { Modes.Background, Animations.MainMenuButtonBackground },
            { Modes.Foreground, Animations.MainMenuButtonBackground },
        });
        private const PositionModes _positionMode = PositionModes.Screen;
        private AnimationManager _animationManager;
        private Modes _mode;
        public enum Modes { Background, Foreground }
        public Modes Mode => _mode;
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
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            {
                _mode = Enum.Parse<Modes>(node.Parameters.GetValueOrDefault("Mode", "Background"));
                _animationManager = Entity.Get<AnimationComponent>().Manager;
                _animationManager.Layer = _layers[_mode];
                _animationManager.PositionMode = _positionMode;
                _animationManager.Play((int)_animations[_mode]);
            }
        }
    }
    internal class MainMenu : GumObject, IUserAction, IControl
    {
        private bool _initialized = false;
        private string _dimmerID;
        private mainView _view;
        private Dimmer _dimmer;
        private UAManager _uaManager;
        private RunningStates _state;
        private float _period;
        private float _time;
        private ReadOnlyDictionary<string, ButtonNode> _buttonNodes;
        private float _buttonVisibility
        {
            set
            {
                foreach (var buttonNode in _buttonNodes.Values)
                {
                    buttonNode.Background.Visibility = value;
                }
            }
        }
        private record ButtonNode(MainMenuButton Background);
        public Keys[] ControlKeys => _uaManager.ControlKeys;
        public Buttons[] ControlButtons => _uaManager.ControlButtons;
        public Directions[] ControlThumbsticks => _uaManager.ControlThumbsticks;
        public void UpdateControl(ButtonStates buttonState, Keys key) => _uaManager.UpdateControl(buttonState, key);
        public void UpdateControl(ButtonStates buttonState, Buttons button) => _uaManager.UpdateControl(buttonState, button);
        public void UpdateControl(Directions thumbsticks, Vector2 position) => _uaManager.UpdateControl(thumbsticks, position);
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            Debug.Assert(!_initialized);
            {
                _view = new mainView();
                var menu = _view.menu;
                menu.resume.Click += (object? sender, EventArgs e) => Close();
                menu.exit.Click += (object? sender, EventArgs e) => Pow.Globals.Game.Exit();
            }
            {
                GumManager.Initialize(_view.Visual);
                GumManager.Position = GumManager.Origin;
                GumManager.Layer = Layers.MenuComponent;
                GumManager.PositionMode = PositionModes.Screen;
                GumManager.Visibility = 0;
            }
            {
                foreach (var button in _view.menu.Buttons.Reverse())
                {
                    Globals.DuoRunner.AddEnvironment(new(
                    Position: Vector2.Zero,
                    Vertices: null,
                    Parameters: new(new Dictionary<string, string>()
                    {
                        {"EntityType", "MainMenuButton"},
                        {"ID", $"mm_bg_{button.Message}"},
                        {"Mode", "Background"},
                    })));
                }
                _buttonNodes = null;
            }
            { 
                _dimmer = null;
                _dimmerID = node.Parameters.GetValueOrDefault("DimmerID", "Dimmer");
            }
            {
                Entity.Get<ControlComponent>().Manager.Initialize(this);
                _uaManager = Globals.DuoRunner.UAGenerator.Acquire();
                _uaManager.Initialize(this);
            }
            {
                _time = 0;
                _state = RunningStates.Waiting;
            }
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

            _dimmer ??= Globals.DuoRunner.Environments.OfType<Dimmer>().Where(dimmer => dimmer.ID == _dimmerID).First();

            if (_buttonNodes == null)
            {
                var ids = _view.menu.Buttons.Select(x => x.Message).ToArray();
                var mainMenuButtons = Globals.DuoRunner.Environments.OfType<MainMenuButton>().ToArray();
                if (mainMenuButtons.Length == _view.menu.Buttons.Length)
                {
                    // Associate button id with main menu button.
                    var buttonNodes = new Dictionary<string, ButtonNode>();
                    foreach (var mainMenuButton in mainMenuButtons)
                    {
                        var buttonIdComponents = mainMenuButton.ID.Split("_");
                        Debug.Assert(buttonIdComponents[0] == "mm");
                        var id = buttonIdComponents[2];
                        if (buttonIdComponents[1] == "bg")
                        {
                            Debug.Assert(ids.Contains(id));
                            buttonNodes.Add(id, new(Background: mainMenuButton));
                        }
                    }
                    Debug.Assert(buttonNodes.Keys.Count == _view.menu.Buttons.Length);
                    _buttonNodes = new(buttonNodes);

                    // Initialize each main menu button bg and fg pair.
                    foreach (var button in _view.menu.Buttons)
                    {
                        var buttonNode = _buttonNodes[button.Message];
                        var position = new Vector2(x: button.Visual.AbsoluteX, y: button.Visual.AbsoluteY);
                        buttonNode.Background.Position = position;
                        buttonNode.Background.Visibility = 0;
                    }
                }
            }

#if DEBUG
            if (!_initialized)
            {
                var mainMenus = Globals.DuoRunner.Environments.OfType<MainMenu>().ToArray();
                Debug.Assert(mainMenus.Length == 1);
            }
#endif

            if (!_initialized && _dimmer != null && _buttonNodes != null)
                _initialized = true;

            if (_state == RunningStates.Starting)
            {
                var visibility = MathHelper.Lerp(1, 0, _time / _period);
                GumManager.Visibility = visibility;
                _buttonVisibility = visibility;
            }
            else if (_state == RunningStates.Stopping)
            {
                var visibility = MathHelper.Lerp(0, 1, _time / _period);
                GumManager.Visibility = visibility;
                _buttonVisibility = visibility;
            }

            if (_time > 0)
                _time -= Pow.Globals.GameTime.GetElapsedSeconds();
            else if (_state == RunningStates.Starting && _dimmer.State == RunningStates.Running)
                ForceOpen();
            else if (_state == RunningStates.Stopping && _dimmer.State == RunningStates.Waiting)
                ForceClose();
        }
        public void UpdateUserAction(int actionId, ButtonStates buttonState, float strength)
        {
            if (!_initialized) return;
            var control = (Controls)actionId;
            if (control == Controls.Menu && buttonState == ButtonStates.Pressed && _state == RunningStates.Waiting)
                Open();
            if (control == Controls.Menu && buttonState == ButtonStates.Pressed && _state == RunningStates.Running)
                Close();
        }
        public RunningStates State => _state;
        public void Open()
        {
            Debug.Assert(_initialized);
            Debug.Assert(_state == RunningStates.Waiting);
            Debug.Assert(_dimmer.State == RunningStates.Waiting);
            Debug.Assert(!Pow.Globals.GamePaused);
            Debug.Assert(!_view.menu.ButtonFocused);
            Pow.Globals.GamePause();
            GumManager.Visibility = 0;
            _buttonVisibility = 0;
            _dimmer.Start();
            _period = _dimmer.Period;
            _time = _dimmer.Period;
            _state = RunningStates.Starting;
        }
        public void Close()
        {
            Debug.Assert(_initialized);
            Debug.Assert(_state == RunningStates.Running);
            Debug.Assert(_dimmer.State == RunningStates.Running);
            Debug.Assert(Pow.Globals.GamePaused);
            Debug.Assert(_view.menu.ButtonFocused);
            var menu = _view.menu;
            menu.ResetFocus();
            GumManager.Visibility = 1;
            _buttonVisibility = 1;
            _dimmer.Stop();
            _period = _dimmer.Period;
            _time = _dimmer.Period;
            _state = RunningStates.Stopping;
        }
        private void ForceOpen()
        {
            var menu = _view.menu;
            menu.ResetFocus();
            menu.resume.IsFocused = true;
            GumManager.Visibility = 1;
            _buttonVisibility = 1;
            _time = 0;
            _state = RunningStates.Running;
        }
        private void ForceClose()
        {
            Pow.Globals.GameResume();
            var menu = _view.menu;
            menu.ResetFocus();
            GumManager.Visibility = 0;
            _buttonVisibility = 0;
            _time = 0;
            _state = RunningStates.Waiting;
        }
    }
}
