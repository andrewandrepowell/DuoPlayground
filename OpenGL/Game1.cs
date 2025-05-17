using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pow.Utilities;
using Pow;
using Duo.Data;

#if DEBUG
using System.Runtime.InteropServices;
#endif

namespace OpenGLGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
#if DEBUG
        [DllImport("kernel32")]
        static extern bool AllocConsole();
#endif
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
#if DEBUG
            AllocConsole();
#endif
        }

        protected override void Initialize()
        {
            base.Initialize();
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Globals.InitializeMonoGame(
                spriteBatch: _spriteBatch,
                graphicsDeviceManager: _graphics,
                game: this);
            Globals.InitializePow(new Data());

            //Globals.Runner.Camera.Zoom = 2f;
            //Globals.Runner.Camera.Position = new Vector2(-32, -32);
            //Globals.Runner.Camera.Rotation = (float)Math.PI * 0.05f;

            Globals.Runner.CreateEntity((int)EntityTypes.DuoRunner);
            Globals.Runner.Map.Load((int)Maps.LevelDebug0);
        }
        protected override void EndRun()
        {
            Globals.Dispose();
            base.EndRun();
        }
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            Globals.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            Globals.Draw();
            base.Draw(gameTime);
        }
    }
}
