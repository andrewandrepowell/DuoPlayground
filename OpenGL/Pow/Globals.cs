using Arch.LowLevel;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

namespace Pow
{
    public static class Globals
    {
        private static States _state = States.WaitingForInitMG;
        private static SpriteBatch _spriteBatch;
        private static GraphicsDeviceManager _graphicsDeviceManager;
        private static Game _game;
        private static GameTime _gameTime = new();
        private static Runner _runner;
        public enum States { WaitingForInitMG, WaitingForInitPow, GameRunning, Disposed }
        public static SpriteBatch SpriteBatch
        {
            get
            {
                Debug.Assert(_state > States.WaitingForInitMG);
                return _spriteBatch;
            }
        }
        public static GraphicsDeviceManager GraphicsDeviceManager
        {
            get
            {
                Debug.Assert(_state > States.WaitingForInitMG);
                return _graphicsDeviceManager;
            }
        }
        public static Game Game
        {
            get
            {
                Debug.Assert(_state > States.WaitingForInitMG);
                return _game;
            }
        }
        public static GameTime GameTime
        {
            get
            {
                Debug.Assert(_state == States.GameRunning);
                return _gameTime;
            }
        }
        public static Runner Runner
        {
            get
            {
                Debug.Assert(_state == States.GameRunning);
                return _runner;
            }
        }
        public static States State => _state;
        public static void InitializeMonoGame(
            SpriteBatch spriteBatch,
            GraphicsDeviceManager graphicsDeviceManager,
            Game game)
        {
            Debug.Assert(_state == States.WaitingForInitMG);
            _spriteBatch = spriteBatch;
            _graphicsDeviceManager = graphicsDeviceManager;
            _game = game;
            _state = States.WaitingForInitPow;
        }
        public static void InitializePow(IRunnerParent runnerParent)
        {
            Debug.Assert(_state == States.WaitingForInitPow);
            _runner = new(runnerParent);
            _state = States.GameRunning;
        }
        public static void Update(GameTime gameTime)
        {
            Debug.Assert(_state == States.GameRunning);
            _gameTime = gameTime;
            _runner.Update();
        }
        public static void Draw()
        {
            Debug.Assert(_state == States.GameRunning);
            _runner.Draw();
        }
        public static void Dispose()
        {
            Debug.Assert(_state == States.GameRunning);
            _runner.Dispose();
            _state = States.Disposed;
        }
    }
}
