using Pow.Utilities.GO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Utilities.Shaders
{
    public class FeatureGenerator : IGOGenerator<IFeature>
    {
        private bool _initialized = false;
        private readonly Dictionary<Type, Stack<IFeature>> _featureTypeStacks = [];
        private readonly Dictionary<IFeature, Type> _featureFeatureTypes = [];
        private readonly Dictionary<Type, BaseEffect> _effectTypeEffects = [];
        public IFeature Acquire() => throw new NotImplementedException();

        public void Initialize(int capacity = 0)
        {
            Debug.Assert(!_initialized);
            _initialized = true;
        }
        public T1 Acquire<T1, T2>(IParent parent) 
            where T1 : FeatureManager<T2>, new() 
            where T2 : BaseEffect, new()
        {
            Debug.Assert(_initialized);
            var featureType = typeof(T1);
            var effectType = typeof(T2);
            if (!_featureTypeStacks.ContainsKey(featureType))
                _featureTypeStacks[featureType] = [];
            var stack = _featureTypeStacks[featureType];
            if (stack.Count == 0)
                stack.Push(new T1());
            if (!_effectTypeEffects.ContainsKey(effectType))
                _effectTypeEffects[effectType] = new T2();
            T1 feature = (T1)stack.Pop();
            T2 effect = (T2)_effectTypeEffects[effectType];
            _featureFeatureTypes[feature] = featureType;
            feature.Initialize(generator: this, effect: effect, parent: parent);
            return feature;
        }
        public void Return(IFeature feature)
        {
            Debug.Assert(_initialized);
            _featureTypeStacks[_featureFeatureTypes[feature]].Push(feature);
        }
    }
}
