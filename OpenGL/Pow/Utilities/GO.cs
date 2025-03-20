using Pow.Utilities.Animations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Utilities.GO
{
    public interface IGOManager
    {
        public void Return();
    }
    public interface IGOGenerator<T> where T : IGOManager
    {
        public void Initialize(int capacity);
        public T Acquire();
    }
    public class GOContainer
    {
        private readonly Dictionary<Type, IGOGenerator<IGOManager>> _generators = [];
        public void Add<T>() where T : IGOGenerator<IGOManager>, new()
        {
            var type = typeof(T);
            Debug.Assert(!_generators.ContainsKey(type));
            _generators.Add(type, new T());
        }
        public T Get<T>() where T : IGOGenerator<IGOManager>
        {
            var type = typeof(T);
            return (T)_generators[type];
        }
    }
}
