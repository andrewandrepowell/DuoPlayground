using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pow
{
    public static class Globals
    {
        private static States _state = States.WaitingForInitMG;
        private static SpriteBatch _spriteBatch;
        private static ContentManager _contentManager;
        private static Runner _runner;
        public enum States { WaitingForInitMG, WaitingForInitPow, GameRunning, Disposed }
        public static SpriteBatch SpriteBatch
        {
            get
            {
                Debug.Assert(_state == States.GameRunning);
                return _spriteBatch;
            }
        }
        public static ContentManager ContentManager
        {
            get
            {
                Debug.Assert(_state == States.GameRunning);
                return _contentManager;
            }
        }
        public static States State => _state;
        public static void InitializeMonoGame(SpriteBatch spriteBatch, ContentManager contentManager)
        {
            Debug.Assert(_state == States.WaitingForInitMG);
            _spriteBatch = spriteBatch;
            _contentManager = contentManager;
            _state = States.WaitingForInitPow;
        }
        public static void InitializePow()
        {
            Debug.Assert(_state == States.WaitingForInitPow);
            _runner = new();
            _state = States.GameRunning;
        }
        public static void Dispose()
        {
            Debug.Assert(_state == States.GameRunning);
            _runner.Dispose();
            _state = States.Disposed;
        }
    }
}
