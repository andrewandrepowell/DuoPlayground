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
    public class NullEffect : BaseEffect
    {
        public override Effect Effect => null;
    }
    public interface IFeature : IGOManager
    {
        public Layers Layer { get; set; }
        public BaseEffect Effect { get; }
        public Vector2 DrawOffset { get; }
        public float Visibility { get; }
        public float Scale { get; }
        public float Rotation { get; }
        public bool Show { get; }
        public void UpdateEffect(in Matrix viewProjection);
        public void Update();
    }
    public interface IParent
    {
        public Texture2D Texture { get; }
        public Rectangle Region { get; }
        public void UpdateDrawOffset();
        public void UpdateVisibility();
        public void UpdateScale();
        public void UpdateRotation();
    }
    public abstract class FeatureManager<T> : IFeature where T : BaseEffect
    {
        private T _effect;
        private IParent _parent;
        private FeatureGenerator _generator;
        private bool _initialized = false;
        public abstract Layers Layer { get; set; }
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
        public virtual float Visibility => 1;
        public virtual float Scale => 1;
        public virtual float Rotation => 0;
        public virtual bool Show => true;
        public virtual void UpdateEffect(in Matrix viewProjection)
        {
            Debug.Assert(_initialized);
        }
        public virtual void Update()
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
            Initialize();
        }
        public void Return()
        {
            Debug.Assert(_initialized);
            Cleanup();
            _generator.Return(this);
            _initialized = false;
        }
        protected virtual void Initialize()
        {
            Debug.Assert(_initialized);
        }
        protected virtual void Cleanup()
        {
            Debug.Assert(_initialized);
        }
    }
}
