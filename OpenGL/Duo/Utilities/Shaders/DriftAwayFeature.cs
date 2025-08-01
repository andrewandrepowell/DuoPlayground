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
    internal class DriftAwayFeature : FeatureManager<NullEffect>
    {
        private const float _directionSpan = MathHelper.TwoPi / 4.0f;
        private const float _speed = 64f;
        private readonly static Random _random = new();
        private readonly static Vector2[] _directions;
        private readonly static Vector2 _gravity = new(0, 128.0f + 64.0f);
        private bool _running;
        private Layers _layer;
        private Vector2 _velocity;
        private Vector2 _drawOffset;
        static DriftAwayFeature()
        {
            _directions = new Vector2[16];
            for (var i = 0; i < _directions.Length; i++)
            {
                var val0 = ((float)i / _directions.Length) * _directionSpan;
                var val1 = (MathHelper.Pi - _directionSpan) / 2.0f + val0;
                var x = (float)System.Math.Cos(val1);
                var y = (float)-System.Math.Sin(val1);
                _directions[i] = new Vector2(x, y);
            }
        }
        public override Layers Layer { get => _layer; set => _layer = value; }
        public override Vector2 DrawOffset => _drawOffset;
        public override bool Show => false;
        public bool Running => _running;
        protected override void Initialize()
        {
            base.Initialize();
            _velocity = _speed * _directions[_random.Next(_directions.Length)];
            _drawOffset = Vector2.Zero;
            _running = false;
            Parent.UpdateDrawOffset();
        }
        public void Start()
        {
            _drawOffset = Vector2.Zero;
            _running = true;
            Parent.UpdateDrawOffset();
        }
        public void Stop()
        {
            _drawOffset = Vector2.Zero;
            _running = false;
            Parent.UpdateDrawOffset();
        }
        public override void Update()
        {
            base.Update();
            if (!_running) return;
            var timeElapsed = Pow.Globals.GameTime.GetElapsedSeconds();
            _drawOffset += _velocity * timeElapsed;
            _velocity += _gravity * timeElapsed;
            Parent.UpdateDrawOffset();
        }
    }
}
