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
    internal class FloatFeature : FeatureManager<NullEffect>
    {
        private static readonly Random _random = new();
        private const float _period = 1.0f;
        private const float _maxHeight = 4.0f;
        private float _time;
        private bool _up;
        private bool _initialized;
        private bool _running;
        private Vector2 _drawOffset;
        private Layers _layer;
        public override Layers Layer { get => _layer; set => _layer = value; }
        public override Vector2 DrawOffset => _drawOffset;
        public override bool Show => false;
        public bool Running => _running;
        protected override void Initialize()
        {
            base.Initialize();
            _up = false;
            _initialized = false;
            _time = _random.NextSingle() * 4;
            _drawOffset.Y = -_maxHeight;
            _running = true;
            Parent.UpdateDrawOffset();
        }
        public void Stop()
        {
            _up = false;
            _initialized = false;
            _time = 0;
            _drawOffset.Y = 0;
            _running = false;
            Parent.UpdateDrawOffset();
        }
        public override void Update()
        {
            base.Update();

            if (!_running) return;

            if (_initialized)
            {
                var timeRatio = System.Math.Max(_time, 0) / _period;
                var newHeight = (_up) ? 
                    MathHelper.Lerp(_maxHeight, -_maxHeight, timeRatio) : 
                    MathHelper.Lerp(-_maxHeight, _maxHeight, timeRatio);
                _drawOffset.Y = newHeight;
                Parent.UpdateDrawOffset();
            }

            if (_time <= 0)
            {
                _initialized = true;
                _up = !_up;
                _time = _period;
            }
            _time -= Pow.Globals.GameTime.GetElapsedSeconds();
            Parent.UpdateDrawOffset();
        }
    }
}
