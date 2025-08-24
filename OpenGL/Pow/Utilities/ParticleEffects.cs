using MonoGame.Extended;
using MonoGame.Extended.Particles;
using Pow.Utilities.GO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Utilities.ParticleEffects
{
    public delegate ParticleEffect CreateParticleEffect();
    public class ParticleEffectManager(Dictionary<int, CreateParticleEffect> createParticleEffects) : IGOManager
    {
        private enum States { Waiting, Initialized, Returned };
        private States _state = States.Waiting;
        private Layers _layer;
        private PositionModes _positionModes;
        private ParticleEffect _effect;
        private Dictionary<int, CreateParticleEffect> _createParticleEffects = createParticleEffects;
        private bool _pausable = true;
        private bool _show = true;
        public ParticleEffect Effect
        {
            get
            {
                Debug.Assert(_state == States.Initialized);
                return _effect;
            }
        }
        public Layers Layer { get => _layer; set => _layer = value; }
        public PositionModes PositionMode { get => _positionModes; set => _positionModes = value; }
        public bool Pausable { get => _pausable; set => _pausable = value; }
        public bool Show { get => _show; set => _show = value; }
        public void Initialize(int id)
        {
            Debug.Assert(_state == States.Waiting);
            Debug.Assert(_createParticleEffects.ContainsKey(id));
            _effect = _createParticleEffects[id]();
            _state = States.Initialized;
        }
        public void Return()
        {
            Debug.Assert(_state == States.Initialized);
            _effect.Dispose();
            _effect = null;
            _state = States.Returned;
        }
        public void Update()
        {
            if (_state != States.Initialized) return;
            if (_pausable && Globals.GamePaused) return;
            _effect.Update(Globals.GameTime.GetElapsedSeconds());
        }
        public void Draw()
        {
            if (_state != States.Initialized) return;
            Globals.SpriteBatch.Draw(_effect);
        }
    }
    public class ParticleEffectGenerator : IGOGenerator<ParticleEffectManager>
    {
        private bool _initialized = false;
        private Dictionary<int, CreateParticleEffect> _createParticleEffects = [];
        public void Configure(int id, CreateParticleEffect createParticleEffect)
        {
            Debug.Assert(!_initialized);
            Debug.Assert(!_createParticleEffects.ContainsKey(id));
            _createParticleEffects.Add(id, createParticleEffect);
        }
        public ParticleEffectManager Acquire()
        {
            Debug.Assert(_initialized);
            return new ParticleEffectManager(_createParticleEffects);
        }
        public void Initialize(int capacity = 0)
        {
            Debug.Assert(!_initialized);
            _initialized = true;
        }
    }
}
