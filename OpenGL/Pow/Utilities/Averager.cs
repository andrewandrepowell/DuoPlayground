using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Pow.Utilities
{
    public class VectorAverage
    {
        private readonly int _amount;
        private readonly float _period;
        private readonly Queue<Vector2> _values;
        private float _time;
        public VectorAverage(float period, int amount)
        {
            Debug.Assert(period > 0);
            Debug.Assert(amount > 0);
            _period = period;
            _amount = amount;
            _time = period;
            _values = [];
        }
        public Vector2 Get()
        {
            if (_values.Count == 0)
                return default;
            Vector2 sum = Vector2.Zero;
            foreach (var value in _values)
                sum += value;
            Vector2 average = sum / _values.Count;
            return average;
        }
        public void Clear()
        {
            _values.Clear();
            _time = _period;
        }
        public void Update(float timeElapsed, Vector2 value)
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
    public class Averager<T> where T : INumber<T>
    {
        private readonly int _amount;
        private readonly float _period;
        private readonly Queue<T> _values;
        private float _time;
        public Averager(float period, int amount)
        {
            Debug.Assert(period > 0);
            Debug.Assert(amount > 0);
            _period = period;
            _amount = amount;
            _time = period;
            _values = [];
        }
        public T Get()
        {
            if (_values.Count == 0)
                return default;
            T sum = T.Zero;
            foreach (var value in _values)
                sum += value;
            T average = sum / T.CreateChecked(_values.Count);
            return average;
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
