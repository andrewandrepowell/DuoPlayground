using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tiled;
using Pow.Utilities;
using Pow;
using System.Collections;
using System.Collections.Generic;
using MonoGame.Extended.Collections;
using Arch.LowLevel;
using Pow.Utilities.Animations;

namespace OpenGLGame
{
    public class Game1 : Game, IRunnerParent
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Queue<int> _csQueue = [];
        private Deque<int> _mgeQueue = [];
        private UnsafeQueue<int> _archQueue = new(64);
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
            map.Configure(0, "tiled/test_map_0");
        }
        public void Initialize(AnimationGenerator generator)
        {

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

            {
                var total = 10000;
        //private Queue<int> _csQueue = [];
        //private Deque<int> _mgeQueue = [];
        //private UnsafeQueue<int> _archQueue = [];
                void CSTest()
                {
                    for (var i = 0; i < total; i++)
                        _csQueue.Enqueue(i);
                    for (var i = 0; i < total; i++)
                        _csQueue.Dequeue();
                }
                void MGETest()
                {
                    for (var i = 0; i < total; i++)
                        _mgeQueue.AddToBack(i);
                    for (var i = 0; i < total; i++)
                        _mgeQueue.RemoveFromFront(out var j);
                }
                void ArchTest()
                {
                    for (var i = 0; i < total; i++)
                        _archQueue.Enqueue(i);
                    for (var i = 0; i < total; i++)
                        _archQueue.Dequeue();
                }

                ArchTest();
                MGETest();
                CSTest();
                
                
            }

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
