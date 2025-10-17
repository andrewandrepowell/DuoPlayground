using Arch.Core.Extensions;
using Duo.Data;
using Duo.Utilities.Shaders;
using DuoGum.Components;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGameGum.Forms.Controls;
using Pow.Components;
using Pow.Utilities;
using Pow.Utilities.Animations;
using Pow.Utilities.Control;
using Pow.Utilities.Gum;
using Pow.Utilities.ParticleEffects;
using Pow.Utilities.UA;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers;

internal class MainMenuButton : Environment
{
    private static readonly ReadOnlyDictionary<Modes, Layers> _layers = new(new Dictionary<Modes, Layers>() 
    {
        { Modes.Background, Layers.FarMenu },
        { Modes.Foreground, Layers.CloseMenu },
    });
    private static readonly ReadOnlyDictionary<Modes, Animations> _animations = new(new Dictionary<Modes, Animations>()
    {
        { Modes.Background, Animations.MainMenuButtonBackground },
        { Modes.Foreground, Animations.MainMenuButtonForeground },
    });
    private const PositionModes _positionMode = PositionModes.Screen;
    private AnimationManager _animationManager;
    private Modes _mode;
    private WindedFeature _windedFeature;
    private bool _selected;
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
    public bool Selected
    {
        get => _selected;
        set
        {
            if (_selected == value) return;
            _selected = value;
            if (_selected)
            {
                _windedFeature.GlowIntensity = 1.0f;
                _windedFeature.EffectScale = 1.1f;
            }
            else
            {
                _windedFeature.GlowIntensity = 0.0f;
                _windedFeature.EffectScale = 1.0f;
            }
        }
    }
    public override void Initialize(PolygonNode node)
    {
        base.Initialize(node);
        _mode = Enum.Parse<Modes>(node.Parameters.GetValueOrDefault("Mode", "Background"));
        _animationManager = Entity.Get<AnimationComponent>().Manager;
        _animationManager.Pauseable = false;
        _animationManager.Layer = _layers[_mode];
        _animationManager.PositionMode = _positionMode;
        _animationManager.Play((int)_animations[_mode]);
        _animationManager.Show = false;
        _windedFeature = _animationManager.CreateFeature<WindedFeature, WindedEffect>();
        _windedFeature.Layer = _layers[_mode];
        _windedFeature.GlowIntensity = 0.0f;
        _selected = false;
    }
}
internal class MainMenu : GumObject, IUserAction, IControl
{
    private const float _dimmerDimness = 0.5f;
    private const float _dimmerPeriod = 0.25f;
    private bool _initialized;
    private string _dimmerID;
    private Dimmer _dimmer;
    private string _branchesID;
    private OptionsMenu _options;
    private string _optionsID;
    private TransitionBranches _branches;
    private mainView _view;
    private UAManager _uaManager;
    private ParticleEffectManager _particleEffectManager;
    private RunningStates _state;
    private float _period;
    private float _time;
    private ReadOnlyDictionary<string, ButtonNode> _buttonNodes;
    private Action _nextAction;
    private bool _fixFocus;
    private float _buttonVisibility
    {
        set
        {
            foreach (var buttonNode in _buttonNodes.Values)
            {
                buttonNode.Background.Visibility = value;
                buttonNode.Foreground.Visibility = value;
            }
        }
    }
    private record ButtonNode(MainMenuButton Background, MainMenuButton Foreground);
    public Keys[] ControlKeys => _uaManager.ControlKeys;
    public Buttons[] ControlButtons => _uaManager.ControlButtons;
    public Directions[] ControlThumbsticks => _uaManager.ControlThumbsticks;
    public void UpdateControl(ButtonStates buttonState, Keys key) => _uaManager.UpdateControl(buttonState, key);
    public void UpdateControl(ButtonStates buttonState, Buttons button) => _uaManager.UpdateControl(buttonState, button);
    public void UpdateControl(Directions thumbsticks, Vector2 position) => _uaManager.UpdateControl(thumbsticks, position);
    public override void Initialize(PolygonNode node)
    {
        base.Initialize(node);
        {
            _initialized = false;
        }
        {
            _view = new mainView();
            var menu = _view.menu;
            menu.resume.Click += (object? sender, EventArgs e) => Close();
            menu.title.Click += (object? sender, EventArgs e) => Transition(Title);
            menu.options.Click += (object? sender, EventArgs e) => OpenOptions();
            menu.exit.Click += (object? sender, EventArgs e) => Transition(Pow.Globals.Game.Exit);
            _fixFocus = false;
            menu.exit.LostFocus += (object? sender, EventArgs e) => FixFocus();
        }
        {
            GumManager.Initialize(_view.Visual);
            GumManager.Position = GumManager.Origin;
            GumManager.Layer = Layers.MidMenu;
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
                Globals.DuoRunner.AddEnvironment(new(
                    Position: Vector2.Zero,
                    Vertices: null,
                    Parameters: new(new Dictionary<string, string>()
                    {
                        {"EntityType", "MainMenuButton"},
                        {"ID", $"mm_fg_{button.Message}"},
                        {"Mode", "Foreground"},
                    })));
            }
            _buttonNodes = null;
        }
        {
            _particleEffectManager = Entity.Get<ParticleEffectComponent>().Manager;
            _particleEffectManager.Initialize(id: (int)ParticleEffects.MenuWind);
            _particleEffectManager.Layer = Layers.FarFront;
            _particleEffectManager.PositionMode = PositionModes.Screen;
            _particleEffectManager.Pausable = false;
            _particleEffectManager.Show = false;
            var effect = _particleEffectManager.Effect;
            effect.Position = new(x: 0, y: Globals.GameWindowSize.Height * 0.4f);
        }
        {
            _dimmer = null;
            _dimmerID = node.Parameters.GetValueOrDefault("DimmerID", "Dimmer");
        }
        {
            _branches = null;
            _branchesID = node.Parameters.GetValueOrDefault("BranchesID", "Branches");
        }
        {
            _options = null;
            _optionsID = node.Parameters.GetValueOrDefault("OptionsID", "Options");
        }
        {
            Entity.Get<ControlComponent>().Manager.Initialize(this);
            _uaManager = Globals.DuoRunner.UAGenerator.Acquire();
            _uaManager.Initialize(this);
        }
        {
            _nextAction = null;
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
        foreach (var mainMenuButton in _buttonNodes.Values)
        {
            var duoRunner = Globals.DuoRunner;
            duoRunner.RemoveEnvironment(mainMenuButton.Background);
            duoRunner.RemoveEnvironment(mainMenuButton.Foreground);
        }
        base.Cleanup();
    }
    public override void Update()
    {
        base.Update();

        _dimmer ??= Globals.DuoRunner.Environments.OfType<Dimmer>().Where(dimmer => dimmer.ID == _dimmerID).First();

        if (_branches == null)
        {
            var branches = Globals.DuoRunner.Environments.OfType<TransitionBranches>().Where(branches => branches.ID == _branchesID).First();
            if (branches.Initialized)
            {
                _branches = branches;
                Debug.Assert(_branches.State == RunningStates.Waiting);
            }
        }

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

        if (_buttonNodes == null)
        {
            var ids = _view.menu.Buttons.Select(x => x.Message).ToArray();
            var mainMenuButtons = Globals.DuoRunner.Environments.OfType<MainMenuButton>().ToArray();
            if (mainMenuButtons.Length == _view.menu.Buttons.Length * 2)
            {
                // Associate button id with main menu button.
                var buttonNodes = new Dictionary<string, ButtonNode>();
                foreach (var id in ids)
                {
                    var mainMenuButtonFg = mainMenuButtons.Where(x => x.ID == $"mm_fg_{id}").First();
                    var mainMenuButtonBg = mainMenuButtons.Where(x => x.ID == $"mm_bg_{id}").First();
                    buttonNodes.Add(id, new(Background: mainMenuButtonBg, Foreground: mainMenuButtonFg));
                }
                Debug.Assert(buttonNodes.Keys.Count == _view.menu.Buttons.Length);
                _buttonNodes = new(buttonNodes);

                // Initialize each main menu button bg and fg pair.
                foreach (var button in _view.menu.Buttons)
                {
                    var buttonNode = _buttonNodes[button.Message];
                    var position = new Vector2(x: button.Visual.AbsoluteX, y: button.Visual.AbsoluteY);
                    buttonNode.Background.Position = position;
                    buttonNode.Foreground.Position = position;
                }
                _buttonVisibility = 0;
            }
        }

#if DEBUG
        if (!_initialized)
        {
            var mainMenus = Globals.DuoRunner.Environments.OfType<MainMenu>().ToArray();
            Debug.Assert(mainMenus.Length == 1);
        }
#endif

        if (!_initialized && _dimmer != null && _buttonNodes != null && _branches != null && _options != null)
            _initialized = true;

        // Hack solution to resolve the looping of focus on the buttons.
        if (_fixFocus)
        {
            _view.menu.ResetFocus();
            _view.menu.resume.IsFocused = true;
            _fixFocus = false;
        }

        if (_initialized && _state == RunningStates.Running)
            foreach (var button in _view.menu.Buttons)
                _buttonNodes[button.Message].Background.Selected = button.IsFocused;

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
        else if (_state == RunningStates.Running && _branches.State == RunningStates.Waiting && _nextAction != null)
        {
            Pow.Globals.GameResume();
            _nextAction();
            _nextAction = null;
        }
    }
    public void UpdateUserAction(int actionId, ButtonStates buttonState, float strength)
    {
        if (!_initialized || _branches.State != RunningStates.Running) return;
        var control = (Controls)actionId;
        if (control == Controls.Menu && buttonState == ButtonStates.Pressed && _state == RunningStates.Waiting)
            Open();
        if (control == Controls.Menu && buttonState == ButtonStates.Pressed && _state == RunningStates.Running)
            Close();
    }
    public RunningStates State => _state;
    public bool Initialized => _initialized;
    public void Open()
    {
        Debug.Assert(_initialized);
        Debug.Assert(_state == RunningStates.Waiting);
        Debug.Assert(_dimmer.State == RunningStates.Waiting);
        Debug.Assert(!Pow.Globals.GamePaused);
        Debug.Assert(!_view.menu.ButtonFocused);
        Debug.Assert(_branches.State == RunningStates.Running);
        Pow.Globals.GamePause();
        GumManager.Visibility = 0;
        _buttonVisibility = 0;
        _particleEffectManager.Show = true;
        _dimmer.Dimness = _dimmerDimness;
        _dimmer.Period = _dimmerPeriod;
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
        Debug.Assert(_branches.State == RunningStates.Running);
        var menu = _view.menu;
        menu.ResetFocus();
        GumManager.Visibility = 1;
        _buttonVisibility = 1;
        _dimmer.Dimness = _dimmerDimness;
        _dimmer.Period = _dimmerPeriod;
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
        _particleEffectManager.Show = false;
        _time = 0;
        _state = RunningStates.Waiting;
    }
    private void Transition(Action nextAction)
    {
        Debug.Assert(_state == RunningStates.Running);
        Debug.Assert(_nextAction == null);
        Debug.Assert(_branches.State == RunningStates.Running);
        _view.menu.ResetFocus();
        _branches.Close();
        _nextAction = nextAction;
    }
    private void Title()
    {
        Pow.Globals.Runner.Map.LoadNext((int)Maps.Title);
    }
    private void OpenOptions()
    {
        Debug.Assert(_initialized);
        Debug.Assert(_state == RunningStates.Running);
        Debug.Assert(_dimmer.State == RunningStates.Running);
        Debug.Assert(Pow.Globals.GamePaused);
        Debug.Assert(_view.menu.ButtonFocused);
        Debug.Assert(_branches.State == RunningStates.Running);
        var menu = _view.menu;
        menu.ResetFocus();
        _options.Open();
    }
    private void CloseOptions()
    {
        Debug.Assert(_initialized);
        Debug.Assert(_state == RunningStates.Running);
        Debug.Assert(_dimmer.State == RunningStates.Running);
        Debug.Assert(Pow.Globals.GamePaused);
        Debug.Assert(!_view.menu.ButtonFocused);
        Debug.Assert(_branches.State == RunningStates.Running);
        var menu = _view.menu;
        menu.options.IsFocused = true;
    }
    private void FixFocus()
    {
        if (!_view.menu.ButtonFocused && _state == RunningStates.Running)
            _fixFocus = true;
    }
}
