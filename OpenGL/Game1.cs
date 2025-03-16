using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tiled;
using Pow.Utilities;
using Pow;

namespace OpenGLGame
{
    public class Game1 : Game, IRunnerParent
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }
        public void Initialize(Map map)
        {
            map.Configure("tiled/test_map_0");
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Globals.InitializeMonoGame(spriteBatch: _spriteBatch, contentManager: Content);
            Globals.InitializePow(this);
        }
        protected override void EndRun()
        {
            base.EndRun();
            Globals.Dispose();
        }
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            Globals.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            Globals.Draw();
            base.Draw(gameTime);
        }
    }
}
