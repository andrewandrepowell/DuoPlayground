using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Utilities
{
    internal class Timer
    {
        private const float _timePeriod = 1;
        private float _timeTime = 0;
        private int _actualTime = 0;
        private string _textTime = "00:00";
        private ServiceUpdatedTimeDelegate _serviceUpdatedTime = null;
        private void UpdateTextTime()
        {
            _textTime = $"{ConvertTime(_actualTime)}";
        }
        public delegate void ServiceUpdatedTimeDelegate();
        public const int MaxTime = 99 * 60 + 59;
        public string CurrentTextTime => _textTime;
        public ServiceUpdatedTimeDelegate ServiceUpdatedTime { get => _serviceUpdatedTime; set => _serviceUpdatedTime = value; }
        public int CurrentTime
        {
            get => _actualTime;
            set
            {
                Trace.Assert(value >= 0);
                Trace.Assert(value <= MaxTime);
                if (_actualTime == value)
                    return;
                _actualTime = value;
                _timeTime = _timePeriod;
                UpdateTextTime();
                _serviceUpdatedTime?.Invoke();
            }
        }
        public static string ConvertTime(int time)
        {
            Trace.Assert(time > 0);
            var minutes = time / 60;
            var seconds = time % 60;
            var minText = minutes.ToString("D2");
            var secText = seconds.ToString("D2");
            return $"{minText}:{secText}";
        }
        public void Update()
        {
            while (_timeTime <= 0 && _actualTime < MaxTime)
            {
                _timeTime += _timePeriod;
                _actualTime++;
                UpdateTextTime();
                _serviceUpdatedTime?.Invoke();
            }
            _timeTime -= Pow.Globals.GameTime.GetElapsedSeconds();
        }
    }
}
