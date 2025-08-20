using Pow.Utilities;
using Pow.Utilities.Shaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Extended;

namespace Duo.Utilities.Shaders
{
    public class PulseGlowFeature : FeatureManager<PulseGlowEffect>
    {
        private const float _period = 4.0f;
        private float _time;
        private Layers _layer;
        private Color _color;
        private RunningStates _state = RunningStates.Waiting;
        public override Layers Layer { get => _layer; set => _layer = value; }
        public Color Color { get => _color; set => _color = value; }
        public override bool Show => _state != RunningStates.Waiting;
        protected override void Initialize()
        {
            base.Initialize();
            _color = default;
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
        public override void UpdateEffect(in Matrix viewProjection)
        {
            base.UpdateEffect(in viewProjection);
            GetEffect().Configure(
                color: _color,
                spriteSize: new(
                    width: Parent.Texture.Width,
                    height: Parent.Texture.Height),
                time: System.Math.Min(_time, _period),
                period: _period);
        }
    }
}
