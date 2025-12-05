using Duo.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pow;
using Pow.Utilities;
using System;
using System.Diagnostics;
using System.IO;


namespace OpenGLGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if DEBUG
            TextWriterTraceListener[] listeners = [
                new TextWriterTraceListener("debug.txt"),
                new TextWriterTraceListener(Console.Out)
            ];
            Trace.Listeners.AddRange(listeners);
            Trace.AutoFlush = true;
            Debug.AutoFlush = true;
#endif
        }

        protected override void Initialize()
        {
            base.Initialize();
        }
        protected override void LoadContent()
        {
            try
            {
                _spriteBatch = new SpriteBatch(GraphicsDevice);
                Globals.InitializeMonoGame(
                    spriteBatch: _spriteBatch,
                    graphicsDeviceManager: _graphics,
                    game: this);
                Globals.InitializePow(new Data());
                Globals.Runner.CreateEntity((int)EntityTypes.DuoRunner);
                // Globals.Runner.Map.Load((int)Maps.LevelDebug2);
                Globals.Runner.Map.Load((int)Maps.Title);
            }
            catch (Exception e)
            { 
                using (StreamWriter writer = new StreamWriter(path: "error_log.txt", append: true))
                {
                    writer.WriteLine($"Exception Message: {e.Message}");
                    writer.WriteLine($"Exception Stacktrace: {e.StackTrace}");
                    writer.WriteLine("-------------------------------------");
                }
                throw;
            }
        }
        protected override void EndRun()
        {
            Globals.Dispose();
            base.EndRun();
        }
        protected override void Update(GameTime gameTime)
        {
            try
            {
                Globals.Update(gameTime);
                base.Update(gameTime);
            }
            catch (Exception e)
            {
                using (StreamWriter writer = new StreamWriter(path: "error_log.txt", append: true))
                {
                    writer.WriteLine($"Exception Message: {e.Message}");
                    writer.WriteLine($"Exception Stacktrace: {e.StackTrace}");
                    writer.WriteLine("-------------------------------------");
                }
                throw;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            try
            {
                GraphicsDevice.Clear(Color.CornflowerBlue);
                Globals.Draw();
                base.Draw(gameTime);
            }
            catch (Exception e)
            {
                using (StreamWriter writer = new StreamWriter(path: "error_log.txt", append: true))
                {
                    writer.WriteLine($"Exception Message: {e.Message}");
                    writer.WriteLine($"Exception Stacktrace: {e.StackTrace}");
                    writer.WriteLine("-------------------------------------");
                }
                throw;
            }
        }
    }
}
