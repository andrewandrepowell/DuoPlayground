using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuoGum.Components;
using Pow.Utilities;
using Pow.Utilities.Animations;
using Pow.Components;
using Arch.Core.Extensions;
using Duo.Data;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using MonoGame.Extended;

namespace Duo.Managers
{
    internal class Dimmer : Environment
    {
        private AnimationManager _animationManager;
        private RunningStates _state;
        private float _period;
        private float _time;
        private float _dimness;
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            _animationManager = Entity.Get<AnimationComponent>().Manager;
            _animationManager.Play((int)Animations.Pixel);
            _animationManager.PositionMode = PositionModes.Screen;
            _animationManager.Layer = Layers.Dimmer;
            _animationManager.Scale = (Vector2)Globals.GameWindowSize;
            _animationManager.Position = Vector2.Zero;
            _animationManager.Color = (Color)typeof(Color).GetProperty(node.Parameters.GetValueOrDefault("Color", "Black")).GetValue(typeof(Color));
            _dimness = float.Parse(node.Parameters.GetValueOrDefault("Period", "0.50"));
            Debug.Assert(_dimness >= 0);
            _period = float.Parse(node.Parameters.GetValueOrDefault("Period", "0.25"));
            Debug.Assert(_period >= 0);
            _time = 0;
            _state = Enum.Parse<RunningStates>(node.Parameters.GetValueOrDefault("State", "Waiting"));
            Debug.Assert(_state == RunningStates.Waiting || _state == RunningStates.Running);
            if (_state == RunningStates.Waiting)
                ForceStop();
            else if (_state == RunningStates.Running)
                ForceStart();
        }
        public override void Update()
        {
            base.Update();

            if (_state == RunningStates.Starting)
                _animationManager.Visibility = MathHelper.Lerp(_dimness, 0, _time/_period);
            else if (_state == RunningStates.Stopping)
                _animationManager.Visibility = MathHelper.Lerp(0, _dimness, _time / _period);

            if (_time > 0)
                _time -= Pow.Globals.GameTime.GetElapsedSeconds();
            else if (_state == RunningStates.Starting)
                ForceStart();
            else if (_state == RunningStates.Stopping)
                ForceStop();
        }
        public float Dimness
        {
            get => _dimness;
            set
            {
                Debug.Assert(value >= 0);
                Debug.Assert(_state == RunningStates.Waiting || _state == RunningStates.Running);
                _dimness = value;
            }
        }
        public float Period
        {
            get => _period;
            set
            {
                Debug.Assert(value >= 0);
                Debug.Assert(_state == RunningStates.Waiting || _state == RunningStates.Running);
                _period = value;
            }
        }
        public RunningStates State => _state;
        public void Start()
        {
            _time = _period;
            _animationManager.Visibility = 0f;
            _state = RunningStates.Starting;
        }
        public void ForceStart()
        {
            _time = 0;
            _animationManager.Visibility = _dimness;
            _state = RunningStates.Running;
        }
        public void Stop()
        {
            _time = _period;
            _animationManager.Visibility = _dimness;
            _state = RunningStates.Stopping;
        }
        public void ForceStop()
        {
            _time = 0;
            _animationManager.Visibility = 0f;
            _state = RunningStates.Waiting;
        }
    }
}
