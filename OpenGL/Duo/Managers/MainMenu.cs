using Arch.Core.Extensions;
using Duo.Utilities;
using DuoGum.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Pow.Components;
using Pow.Utilities;
using Pow.Utilities.Control;
using Pow.Utilities.Gum;
using Pow.Utilities.UA;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class MainMenu : GumObject, IUserAction, IControl
    {
        private bool _initialized;
        private string _dimmerID;
        private mainView _view;
        private Dimmer _dimmer;
        private UAManager _uaManager;
        private RunningStates _state;
        private float _period;
        private float _time;
        public void UpdateControl(ButtonStates buttonState, Keys key) => _uaManager.UpdateControl(buttonState, key);
        public Keys[] ControlKeys => _uaManager.ControlKeys;
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            _view = new mainView();
            var menu = _view.menu;
            menu.resume.Click += (object? sender, EventArgs e) => Close();
            menu.exit.Click += (object? sender, EventArgs e) => Pow.Globals.Game.Exit();
            GumManager.Initialize(_view.Visual);
            GumManager.Position = GumManager.Origin;
            GumManager.Layer = Layers.Menu;
            GumManager.PositionMode = PositionModes.Screen;
            GumManager.Visibility = 0;
            _dimmer = null;
            _dimmerID = node.Parameters.GetValueOrDefault("DimmerID", "Dimmer");
            Entity.Get<ControlComponent>().Manager.Initialize(this);
            _uaManager = Globals.DuoRunner.UAGenerator.Acquire();
            _uaManager.Initialize(this);
            _time = 0;
            _state = RunningStates.Waiting;
            _initialized = false;
        }
        public override void Update()
        {
            base.Update();

            if (_dimmer == null)
                _dimmer = Globals.DuoRunner.Environments.OfType<Dimmer>().Where(dimmer => dimmer.ID == _dimmerID).First();
            if (_dimmer != null && !_initialized)
                _initialized = true;

            if (_state == RunningStates.Starting)
                GumManager.Visibility = MathHelper.Lerp(1, 0, _time / _period);
            else if (_state == RunningStates.Stopping)
                GumManager.Visibility = MathHelper.Lerp(0, 1, _time / _period);

            if (_time > 0)
                _time -= Pow.Globals.GameTime.GetElapsedSeconds();
            else if (_state == RunningStates.Starting && _dimmer.State == RunningStates.Running)
                ForceOpen();
            else if (_state == RunningStates.Stopping && _dimmer.State == RunningStates.Waiting)
                ForceClose();
        }
        public void UpdateUserAction(int actionId, ButtonStates buttonState)
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
            _time = 0;
            _state = RunningStates.Running;
        }
        private void ForceClose()
        {
            Pow.Globals.GameResume();
            var menu = _view.menu;
            menu.ResetFocus();
            GumManager.Visibility = 0;
            _time = 0;
            _state = RunningStates.Waiting;
        }
    }
}
