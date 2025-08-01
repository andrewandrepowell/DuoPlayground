using Microsoft.Xna.Framework;
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
    internal class RotationFeature : FeatureManager<NullEffect>
    {
        private const float _rotationsPerSecond = 4.0f;
        private const float _period = 1.0f / _rotationsPerSecond;
        private float _rotation;
        private Layers _layer;
        private RunningStates _state = RunningStates.Waiting;
        private float _time;
        public override bool Show => false;
        public override float Rotation => _rotation;
        public override Layers Layer { get => _layer; set => _layer = value; }
        public RunningStates State => _state;
        protected override void Initialize()
        {
            base.Initialize();
            _time = 0;
            _rotation = 0;
            _state = RunningStates.Waiting;
            Parent.UpdateRotation();
        }
        public void Start()
        {
            if (_state == RunningStates.Running) return;
            _time = _period;
            _rotation = 0;
            _state = RunningStates.Running;
        }
        public void ForceStop()
        {
            if (_state == RunningStates.Waiting) return;
            _time = 0;
            _rotation = 0;
            _state = RunningStates.Waiting;
            Parent.UpdateRotation();
        }
        public void Stop()
        {
            if (_state == RunningStates.Stopping) return;
            _state = RunningStates.Stopping;
        }
        public override void Update()
        {
            base.Update();
            if (_state == RunningStates.Waiting) return;

            _rotation = _time * _rotationsPerSecond * MathHelper.TwoPi;
            Parent.UpdateRotation();

            while (_time <= 0)
            { 
                if (_state == RunningStates.Stopping)
                {
                    ForceStop();
                    return;
                }
                _time += _period; 
            }
            _time -= Pow.Globals.GameTime.GetElapsedSeconds();
        }
    }
}
