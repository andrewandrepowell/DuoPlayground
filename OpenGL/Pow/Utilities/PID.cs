using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Utilities
{
    public class PID<T> 
        where T : INumber<T>
    {
        // https://en.wikipedia.org/wiki/Proportional%E2%80%93integral%E2%80%93derivative_controller
        private readonly float _period;
        private readonly T _integralCoef;
        private readonly T _proportionalCoef;
        private readonly T _derivativeCoef;
        private readonly T _integralClamp;
        private readonly T _controlClamp;
        private T _prevError;
        private T _totalError;
        private T _controlValue;
        private float _time;
        public PID(float period, T integralCoef, T proportionalCoef, T derivativeCoef, T integralClamp, T controlClamp)
        {
            Debug.Assert(period > 0);
            Debug.Assert(integralClamp > T.Zero);
            Debug.Assert(controlClamp > T.Zero);
            _period = period;
            _integralCoef = integralCoef;
            _proportionalCoef = proportionalCoef;
            _derivativeCoef = derivativeCoef;
            _integralClamp = integralClamp;
            _controlClamp = controlClamp;
            Clear();
        }
        public T Get() => _controlValue;
        public void Clear()
        {
            _prevError = T.Zero;
            _totalError = T.Zero;
            _controlValue = T.Zero;
            _time = _period;
        }
        public void Update(float timeElapsed, T error)
        {
            while (_time <= 0)
            {
                var proportional = _proportionalCoef * error;
                var derivative = _derivativeCoef * (error - _prevError) * T.CreateChecked(_period);
                _totalError = T.Clamp(_totalError + error, -_integralClamp, +_integralClamp);
                var integral = _integralCoef * _totalError;
                _controlValue = T.Clamp(proportional + derivative + integral, -_controlClamp, +_controlClamp);
                _prevError = error;
                _time += _period;
            }
            _time -= timeElapsed;
        }
    }
}
