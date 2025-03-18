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
using MonoGame.Extended;
using System;

namespace OpenGLGame
{
    public class Game1 : Game, IRunnerParent
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Queue<int> _csQueue = [];
        private Deque<int> _mgeQueue = [];
        private UnsafeQueue<int> _archQueue = new(64);
        private AnimationManager _animationManager;
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
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Globals.InitializeMonoGame(spriteBatch: _spriteBatch, contentManager: Content);
            Globals.InitializePow(this);

            Globals.Runner.Camera.Zoom = 2f;
            Globals.Runner.Camera.Position = new Vector2(-32, 0);
            Globals.Runner.Camera.Rotation = (float)Math.PI * 0;
            Globals.Runner.Map.Load(0);

            _animationManager = Globals.Runner.AnimationGenerator.Acquire();
            _animationManager.Play(0);
            _animationManager.Position = new Vector2(x: 64, y: 64);
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
                if (!_animationManager.Running)
                {
                    if (_animationManager.AnimationId == 0)
                    {
                        _animationManager.Play(1);
                    }
                    else
                    {
                        _animationManager.Play(0);
                    }
                }
                _animationManager.Update();
            }
            {
                var total = 10000;
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

            _spriteBatch.Begin(transformMatrix: Globals.Runner.Camera.View);
            //_animationManager.Draw();
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
