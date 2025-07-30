using MonoGame.Extended;
using Pow.Utilities;
using Pow.Utilities.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Utilities.Shaders
{
    public class ShineFeature : FeatureManager<ShineEffect>
    {
        private const float _period = 4.0f;
        private float _time;
        private Layers _layer;
        private RunningStates _state = RunningStates.Waiting;
        public override bool Running => _state != RunningStates.Waiting;
        public override Layers Layer { get => _layer; set => _layer = value; }
        protected override void Initialize()
        {
            base.Initialize();
            _layer = default;
            _time = default;
            _state = RunningStates.Waiting;
        }
        public void Start()
        {
            if (_state == RunningStates.Running)
                return;
            _time = 0;
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
            _state = RunningStates.Waiting;
        }
        public override void Update()
        {
            base.Update();
            if (Pow.Globals.GamePaused)
                return;
            if (_state == RunningStates.Waiting)
                return;
            var timeElapsed = Pow.Globals.GameTime.GetElapsedSeconds();
            if (_time < _period)
            {
                _time += timeElapsed;
            }
            else
            {
                _time -= _period;
                if (_state == RunningStates.Stopping)
                    ForceStop();
            }
        }
        public override void UpdateEffect()
        {
            base.UpdateEffect();
            GetEffect().Configure(
                time: System.Math.Min(_time, _period));
        }
    }
}
