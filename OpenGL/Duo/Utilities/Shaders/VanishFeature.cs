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
    internal class VanishFeature : FeatureManager<NullEffect>
    {
        private const float _period = 1.0f;
        private Layers _layer;
        private float _time;
        private float _visibility;
        private bool _running;
        public override Layers Layer { get => _layer; set => _layer = value; }
        public override bool Show => false;
        public override float Visibility => _visibility;
        public bool Running => _running;
        protected override void Initialize()
        {
            base.Initialize();
            _time = 0;
            _visibility = 1;
            _running = false;
            Parent.UpdateVisibility();
        }
        public void Start()
        {
            _time = _period;
            _visibility = 1;
            _running = true;
            Parent.UpdateVisibility();
        }
        public void Stop()
        {
            _time = 0;
            _visibility = 1;
            _running = false;
            Parent.UpdateVisibility();
        }
        public override void Update()
        {
            base.Update();
            if (!_running) return;
            if (_time <= 0)
            {
                _time = 0;
                _running = false;
            }
            _visibility = _time / _period;
            Parent.UpdateVisibility();
            _time -= Pow.Globals.GameTime.GetElapsedSeconds();
        }
    }
}
