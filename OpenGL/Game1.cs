using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pow.Utilities;
using Pow;
using Duo.Data;


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
            Globals.Runner.CreateEntity((int)EntityTypes.DuoRunner);
            Globals.Runner.Map.Load((int)Maps.LevelDebug1);
        }
        protected override void EndRun()
        {
            Globals.Dispose();
            base.EndRun();
        }
        protected override void Update(GameTime gameTime)
        {
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
