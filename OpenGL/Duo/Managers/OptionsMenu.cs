using Arch.Core.Extensions;
using Duo.Data;
using Duo.Utilities.Shaders;
using DuoGum.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGameGum.Forms.Controls;
using Pow.Components;
using Pow.Utilities;
using Pow.Utilities.Animations;
using Pow.Utilities.Control;
using Pow.Utilities.ParticleEffects;
using Pow.Utilities.UA;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Duo.Managers;

internal class OptionsMenuButton : Environment
{
    private static readonly Layers _layer = Layers.FarOptionsMenu;
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
        _animationManager.Show = true;
        _animationManager.Play((int)Animations.OptionsButtonBushless);
        _animationManager.Visibility = 0;
        _buttonFeature = _animationManager.CreateFeature<TitleMenuGlowButtonFeature, TitleMenuGlowButtonEffect>();
        _buttonFeature.Layer = _layer;
        _buttonFeature.GlowIntensity = 0.0f;
        _buttonFeature.GlowColor = new Color(243.0f, 233.0f, 220.0f, 255.0f);
        _selected = false;
    }
}
internal class OptionsMenu : GumObject, IUserAction, IControl
{
    private const string _assetName = "config/options.json";
    private const float _dimmerDimness = 0.5f;
    private const float _dimmerPeriod = 0.25f;
    private static readonly Layers _layer = Layers.OptionsMenu;
    private static readonly PositionModes _positionMode = PositionModes.Screen;
    private bool _initialized;
    private optionsView _view;
    private ReadOnlyDictionary<string, OptionsMenuButton> _buttons;
    private RunningStates _state;
    private float _period;
    private float _time;
    private string _dimmerID;
    private Dimmer _dimmer;
    private UAManager _uaManager;
    private float _buttonVisibility
    {
        set
        {
            foreach (var button in _buttons.Values)
            {
                button.Visibility = value;
                button.Visibility = value;
            }
        }
    }
    private Action _backAction;
    private ViewModes _viewMode;
    private float _musicVolume;
    private float _sfxVolume;
    private bool _fixFocus;
    private record struct ButtonControl(string ID, Controls Control);
    private ReadOnlyDictionary<ButtonControl, Action> _buttonControlActions;
    public enum ViewModes { Full, Window };
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
            _view = new optionsView();
            GumManager.Initialize(_view.Visual);
            GumManager.Position = GumManager.Origin;
            GumManager.Layer = _layer;
            GumManager.PositionMode = _positionMode;
            GumManager.Visibility = 0;
        }
        {
            Load();
        }
        {
            _view.options.back.Click += (object? sender, EventArgs e) => Close();
            _fixFocus = false;
            _view.options.back.LostFocus += (object? sender, EventArgs e) => FixFocus();
        }
        {
            foreach (var button in _view.options.Buttons)
            {
                Globals.DuoRunner.AddEnvironment(new(
                    Position: Vector2.Zero,
                    Vertices: null,
                    Parameters: new(new Dictionary<string, string>()
                    {
                            {"EntityType", "OptionsMenuButton"},
                            {"ID", $"o_btn_{button.text_bText}"},
                    })));
            }
            _buttons = null;
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
            var buttonControlActions = new Dictionary<ButtonControl, Action>()
            {
                { new(ID: _view.options.music.text_bText, Control: Controls.MoveLeft), TurnDownMusic },
                { new(ID: _view.options.music.text_bText, Control: Controls.MoveRight), TurnUpMusic },
                { new(ID: _view.options.sfx.text_bText, Control: Controls.MoveLeft), TurnDownSFX },
                { new(ID: _view.options.sfx.text_bText, Control: Controls.MoveRight), TurnUpSFX },
                { new(ID: _view.options.fw.text_bText, Control: Controls.MoveLeft), ToggleView },
                { new(ID: _view.options.fw.text_bText, Control: Controls.MoveRight), ToggleView }
            };
            _buttonControlActions = new(buttonControlActions);
        }
        {
            _backAction = null;
        }
        {
            _initialized = false;
            _time = 0;
            _state = RunningStates.Waiting;
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

        _dimmer ??= Globals.DuoRunner.Environments.OfType<Dimmer>().Where(dimmer => dimmer.ID == _dimmerID).First();

        if (_buttons == null)
        {
            var ids = _view.options.Buttons.Select(x => x.text_bText).ToArray();
            var optionMenuButtons = Globals.DuoRunner.Environments.OfType<OptionsMenuButton>().ToArray();
            if (optionMenuButtons.Length == _view.options.Buttons.Length)
            {
                // Associate button id with title menu button.
                var buttons = new Dictionary<string, OptionsMenuButton>();
                foreach (var id in ids)
                {
                    var optionMenuButton = optionMenuButtons.Where(x => x.ID == $"o_btn_{id}").First();
                    buttons.Add(id, optionMenuButton);
                }
                Debug.Assert(buttons.Keys.Count == _view.options.Buttons.Length);
                _buttons = new(buttons);

                // Initialize each title menu button.
                foreach (var gumButton in _view.options.Buttons)
                {
                    var button = _buttons[gumButton.text_bText];
                    var position = new Vector2(x: gumButton.Visual.AbsoluteX, y: gumButton.Visual.AbsoluteY);
                    button.Position = position;
                    button.Rotation = -gumButton.containerRotation / 180.0f * MathHelper.Pi;
                }
            }
        }

        if (!_initialized && _buttons != null && _dimmer != null)
            _initialized = true;

        if (_initialized && _state == RunningStates.Running)
            foreach (var button in _view.options.Buttons)
                _buttons[button.text_bText].Selected = button.IsFocused;

        if (_fixFocus)
        {
            _view.options.ResetFocus();
            _view.options.fw.IsFocused = true;
            _fixFocus = false;
        }

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
        if (!_initialized || _state != RunningStates.Running) return;
        var control = (Controls)actionId;
        foreach (ref var button in _view.options.Buttons.AsSpan())
        {
            if (button.IsFocused && buttonState == ButtonStates.Pressed)
            {
                if (_buttonControlActions.TryGetValue(new(ID: button.text_bText, Control: control), out var action))
                    action();
            }
        }
    }
    public RunningStates State => _state;
    public bool Initialized => _initialized;
    public Action BackAction
    {
        get => _backAction;
        set
        {
            Debug.Assert(_initialized);
            Debug.Assert(_state == RunningStates.Waiting);
            _backAction = value;
        }
    }
    public ViewModes ViewMode
    {
        get => _viewMode;
        set
        {
            if (value == _viewMode) return;
            _viewMode = value;
            switch (_viewMode)
            {
                case ViewModes.Full:
                    {
                        _view.options.fw.box_c_0checkmarkVisible = true;
                        _view.options.fw.box_c_1checkmarkVisible = false;
                    }
                    break;
                case ViewModes.Window:
                    {
                        _view.options.fw.box_c_0checkmarkVisible = false;
                        _view.options.fw.box_c_1checkmarkVisible = true;
                    }
                    break;
            }

        }
    }
    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            if (value == _musicVolume) return;
            _musicVolume = MathHelper.Clamp(value, 0, 100);
            var button = _view.options.music;
            button.pointerX = _musicVolume;
        }
    }
    public float SFXVolume
    {
        get => _sfxVolume;
        set
        {
            if (value == _sfxVolume) return;
            _sfxVolume = MathHelper.Clamp(value, 0, 100);
            var button = _view.options.sfx;
            button.pointerX = _sfxVolume;
        }
    }
    public void Open()
    {
        Debug.Assert(_initialized);
        Debug.Assert(_state == RunningStates.Waiting);
        Debug.Assert(_dimmer.State == RunningStates.Waiting);
        Debug.Assert(!_view.options.ButtonFocused);
        GumManager.Visibility = 0;
        _buttonVisibility = 0;
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
        Debug.Assert(_view.options.ButtonFocused);
        var menu = _view.options;
        menu.ResetFocus();
        GumManager.Visibility = 1;
        _buttonVisibility = 1;
        _dimmer.Dimness = _dimmerDimness;
        _dimmer.Period = _dimmerPeriod;
        _dimmer.Stop();
        _period = _dimmer.Period;
        _time = _dimmer.Period;
        Save();
        _state = RunningStates.Stopping;
    }
    private void ForceOpen()
    {
        var menu = _view.options;
        menu.ResetFocus();
        menu.back.IsFocused = true;
        GumManager.Visibility = 1;
        _buttonVisibility = 1;
        _time = 0;
        _state = RunningStates.Running;
    }
    private void ForceClose()
    {
        var menu = _view.options;
        menu.ResetFocus();
        GumManager.Visibility = 0;
        _buttonVisibility = 0;
        _time = 0;
        _state = RunningStates.Waiting;
        _backAction?.Invoke();
    }
    private void TurnUpMusic() => MusicVolume += 10;
    private void TurnDownMusic() => MusicVolume -= 10;
    private void TurnUpSFX() => SFXVolume += 10;
    private void TurnDownSFX() => SFXVolume -= 10;
    private void ToggleView() => ViewMode = (ViewMode == ViewModes.Full) ? ViewModes.Window : ViewModes.Full;
    private void FixFocus()
    {
        if (!_view.options.ButtonFocused && _state == RunningStates.Running)
            _fixFocus = true;
    }
    private void Load()
    {
        var path = Path.Combine(Pow.Globals.Game.Content.RootDirectory, _assetName);
        var text = File.ReadAllText(path);
        var root = JsonNode.Parse(text);
        MusicVolume = (float)root["MusicVolume"];
        SFXVolume = (float)root["SFXVolume"];
        ViewMode = Enum.Parse<ViewModes>(root["ViewMode"].ToString());
    }
    private void Save()
    {
        var path = Path.Combine(Pow.Globals.Game.Content.RootDirectory, _assetName);
        var root = new JsonObject();
        root["MusicVolume"] = MusicVolume;
        root["SFXVolume"] = SFXVolume;
        root["ViewMode"] = $"{ViewMode}";
        File.WriteAllText(path: path, contents: root.ToJsonString(new System.Text.Json.JsonSerializerOptions() { WriteIndented = true}));
    }
}
