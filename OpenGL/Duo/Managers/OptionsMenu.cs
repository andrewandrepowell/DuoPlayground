using Arch.Core.Extensions;
using Duo.Data;
using Duo.Utilities.Shaders;
using DuoGum.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
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
internal class OptionsMenu : GumObject
{
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
            _view.options.back.Click += (object? sender, EventArgs e) => Close();
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
    public void Open()
    {
        Debug.Assert(_initialized);
        Debug.Assert(_state == RunningStates.Waiting);
        Debug.Assert(_dimmer.State == RunningStates.Waiting);
        Debug.Assert(!_view.options.ButtonFocused);
        Pow.Globals.GamePause();
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
        Pow.Globals.GameResume();
        var menu = _view.options;
        menu.ResetFocus();
        GumManager.Visibility = 0;
        _buttonVisibility = 0;
        _time = 0;
        _state = RunningStates.Waiting;
        _backAction?.Invoke();
    }
}
