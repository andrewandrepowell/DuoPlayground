using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pow.Utilities;
using Pow;
using System;
using MonoGame.Extended.Animations;


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
        public void Initialize(Runner runner)
        {
            runner.Map.Configure(0, "tiled/test_map_0");
            runner.AnimationGenerator.ConfigureSprite(0, "images/test_ball_0", new(32, 32));
            runner.AnimationGenerator.ConfigureAnimation(0, 0, 0, [0, 1, 2, 3], 0.25f, false);
            runner.AnimationGenerator.ConfigureAnimation(1, 0, 1, [0, 4, 5], 0.25f, false);
            runner.AddEntityType(0, [typeof(AnimationComponent)]);
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Globals.InitializeMonoGame(spriteBatch: _spriteBatch, contentManager: Content);
            Globals.InitializePow(this);

            Globals.Runner.Camera.Zoom = 2f;
            Globals.Runner.Camera.Position = new Vector2(-32, -32);
            Globals.Runner.Camera.Rotation = (float)Math.PI * 0.05f;

            Globals.Runner.CreateEntity(0);
            Globals.Runner.Map.Load(0);
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
