using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Utilities
{
    public class Median<T>(float period, int amount) : Median<T, object>(period, amount) where T : INumber<T>
    {
        public new T Get() => base.Get().Value;
        public void Update(float timeElapsed, T value) => base.Update(timeElapsed, value);
    }
    public class Median<T, TObject> 
        where T : INumber<T>
    {
        public readonly record struct Node(T Value, TObject Object);
        private static readonly Comparison<Node> _sorter = (Node x, Node y) => x.Value.CompareTo(y.Value);
        private readonly int _amount;
        private readonly float _period;
        private readonly Queue<Node> _values;
        private readonly List<Node> _sort;
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
        public Node Get()
        {
            if (_values.Count == 0)
                return default;
            _sort.Clear();
            foreach (var item in _values)
                _sort.Add(item);
            _sort.Sort(_sorter);
            var i = _values.Count / 2;
            return _sort[i];
        }
        public void Clear()
        {
            _values.Clear();
            _time = _period;
        }
        public void Update(float timeElapsed, T value, TObject obj = default)
        {
            while (_time <= 0)
            {
                Debug.Assert(_values.Count <= _amount);
                if (_values.Count == _amount)
                    _values.Dequeue();
                _values.Enqueue(new(Value: value, Object: obj));
                _time += _period;
            }
            _time -= timeElapsed;
        }
    }
}
