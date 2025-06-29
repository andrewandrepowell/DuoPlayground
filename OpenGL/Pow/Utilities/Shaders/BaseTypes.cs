using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pow.Utilities.GO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Utilities.Shaders
{
    public abstract class BaseEffect
    {
        public abstract Effect Effect { get; }
    }
    public interface IFeature : IGOManager
    {
        public Layers Layer { get; }
        public BaseEffect Effect { get; }
        public Vector2 DrawOffset { get; }
        public float Visibility { get; }
        public float Scale { get; }
        public float Rotation { get; }
        public void UpdateEffect();
    }
    public interface IParent
    {
        public Texture2D Texture { get; }
        public Rectangle Region { get; }
    }
    public abstract class FeatureManager<T> : IFeature where T : BaseEffect
    {
        private T _effect;
        private IParent _parent;
        private FeatureGenerator _generator;
        private bool _initialized = false;
        public abstract Layers Layer { get; }
        public T GetEffect() => _effect;
        public BaseEffect Effect
        {
            get
            {
                Debug.Assert(_initialized);
                return _effect;
            }
        }
        public IParent Parent
        {
            get
            {
                Debug.Assert(_initialized);
                return _parent;
            }
        }
        public virtual Vector2 DrawOffset => Vector2.Zero;
        public virtual float Visibility => 0;
        public virtual float Scale => 0;
        public virtual float Rotation => 0;
        public virtual void UpdateEffect()
        {
            Debug.Assert(_initialized);
        }
        public void Initialize(FeatureGenerator generator, T effect, IParent parent)
        {
            Debug.Assert(!_initialized);
            _generator = generator;
            _effect = effect;
            _parent = parent;
            _initialized = true;
        }
        public virtual void Return()
        {
            Debug.Assert(_initialized);
            _generator.Return(this);
            _initialized = false;
        }
    }
}
