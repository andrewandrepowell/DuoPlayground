using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Utilities
{
    public class Median<T> where T : INumber<T>
    {
        private readonly int _amount;
        private readonly float _period;
        private readonly Queue<T> _values;
        private readonly List<T> _sort;
        private float _time;
        public Median(float period, int amount)
        {
            Debug.Assert(period > 0);
            Debug.Assert(amount > 0);
            _period = period;
            _amount = amount;
            _time = period;
            _values = [];
            _sort = [];
        }
        public T Get()
        {
            if (_values.Count == 0)
                return default;
            _sort.Clear();
            foreach (var item in _values)
                _sort.Add(item);
            _sort.Sort();
            var i = _values.Count / 2;
            return _sort[i];
        }
        public void Clear()
        {
            _values.Clear();
            _time = _period;
        }
        public void Update(float timeElapsed, T value)
        {
            while (_time <= 0)
            {
                Debug.Assert(_values.Count <= _amount);
                if (_values.Count == _amount)
                    _values.Dequeue();
                _values.Enqueue(value);
                _time += _period;
            }
            _time -= timeElapsed;
        }
    }
}
