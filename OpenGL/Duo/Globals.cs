using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duo.Managers;
using MonoGame.Extended;

namespace Duo
{
    public static class Globals
    {
        private static States _state = States.WaitingForInit;
        private static DuoRunner _duoRunner;
        internal static readonly SizeF GameWindowSize = new(640, 480);
        internal const float PixelsPerMeter = 100;
        internal enum States { WaitingForInit, GameRunning, Disposed }
        internal static DuoRunner DuoRunner
        {
            get
            {
                Debug.Assert(_state == States.GameRunning);
                return _duoRunner;
            }
        }
        internal static void Initialize(DuoRunner duoRunner)
        {
            Debug.Assert(_state == States.WaitingForInit);
            _duoRunner = duoRunner;
            _state = States.GameRunning;
        }
    }
}
