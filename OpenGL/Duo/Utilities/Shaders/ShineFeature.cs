using MonoGame.Extended;
using Pow.Utilities;
using Pow.Utilities.Shaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Utilities.Shaders
{
    public class ShineFeature : FeatureManager<ShineEffect>
    {
        private readonly static Random _random = new();
        private const float _shinePeriod = 4.0f;
        private const float _idleMaxPeriod = 4.0f;
        private const float _idleMinPeriod = 1.0f;
        private const float _speed = 0.15f;
        private float _period;
        private float _time;
        private bool _flashing;
        private Layers _layer;
        private RunningStates _state = RunningStates.Waiting;
        public override bool Show => _state != RunningStates.Waiting && _flashing;
        public override Layers Layer { get => _layer; set => _layer = value; }
        protected override void Initialize()
        {
            base.Initialize();
            _layer = default;
            _time = 0;
            _flashing = false;
            _state = RunningStates.Waiting;
        }
        public void Start()
        {
            Debug.Assert(_idleMaxPeriod >= _idleMinPeriod);
            if (_state == RunningStates.Running)
                return;
            _time = 0;
            _flashing = false;
            _period = _idleMinPeriod + _random.NextSingle() * (_idleMaxPeriod - _idleMinPeriod);
            _state = RunningStates.Running;
        }
        public void Stop()
        {
            if (_state == RunningStates.Waiting || _state == RunningStates.Stopping)
                return;
            _state = RunningStates.Stopping;
        }
        public void ForceStop()
        {
            if (_state == RunningStates.Waiting)
                return;
            _flashing = false;
            _state = RunningStates.Waiting;
        }
        public override void Update()
        {
            base.Update();
            if (_state == RunningStates.Waiting)
                return;
            var timeElapsed = Pow.Globals.GameTime.GetElapsedSeconds();
            if (_time < _period && (!_flashing || _state == RunningStates.Running))
            {
                _time += timeElapsed;
            }
            else if (_state == RunningStates.Stopping)
            {
                ForceStop();
            }
            else
            {
                _time -= _period;
                if (_flashing)
                {
                    _period = _idleMinPeriod + _random.NextSingle() * (_idleMaxPeriod - _idleMinPeriod);
                    _flashing = false;
                }
                else
                {
                    _period = _shinePeriod;
                    _flashing = true;
                }
            }
        }
        public override void UpdateEffect()
        {
            base.UpdateEffect();
            if (!_flashing) return;
            GetEffect().Configure(
                time: System.Math.Min(_time * _speed, _period));
        }
    }
}
