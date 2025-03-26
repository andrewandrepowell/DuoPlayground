using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pow.Utilities;
using Pow;
using System;
using System.Collections.Generic;
using Pow.Components;
using Pow.Utilities.GO;
using Arch.Core;
using Arch.Core.Extensions;
using MonoGame.Extended;
using System.Collections;


namespace OpenGLGame
{
    public class DebugGOManager : GOCustomManager
    {
        private float timer;
        public override void Initialize()
        {
            var animationManager = Entity.Get<AnimationComponent>().Manager;
            animationManager.Play(0);
            timer = 3;
            var status = Entity.Get<StatusComponent>();
            base.Initialize();
        }
        public override void Update()
        {
            var animationManager = Entity.Get<AnimationComponent>().Manager;
            if (!animationManager.Running)
            {
                animationManager.Play(0);
            }
            if (timer > 0)
                timer -= Globals.GameTime.GetElapsedSeconds();
            else
            {
                Globals.Runner.DestroyEntity(in Entity);
            }
            base.Update();
        }
    }
    public class Game1 : Game, IRunnerParent
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private float _debugTimer = 6;
        private Queue<Entity> _debugResponseQueue = [];
        private Queue<Vector2> _debugPositionQueue = [];
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
            runner.AddGOCustomManager<DebugGOManager>();
            runner.AddEntityType(0, (World world) => world.Create(new StatusComponent() { State = EntityStates.Initializing}, new AnimationComponent(), new PositionComponent(), new GOCustomComponent<DebugGOManager>()));
        }
        public void Initialize(Map.MapNode node)
        {

        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Globals.InitializeMonoGame(spriteBatch: _spriteBatch, contentManager: Content);
            Globals.InitializePow(this);

            Globals.Runner.Camera.Zoom = 2f;
            //Globals.Runner.Camera.Position = new Vector2(-32, -32);
            //Globals.Runner.Camera.Rotation = (float)Math.PI * 0.05f;
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

            while (_debugTimer <= 0)
            {
                for (var i = 0; i < 16; i++)
                {
                    //EntityAction lambda = (in Entity entity) => entity.Set<PositionComponent>(new(new(i, i)));
                    Globals.Runner.CreateEntity(0, _debugResponseQueue);
                    _debugPositionQueue.Enqueue(new(i + 64, i + 64));
                    //Globals.Runner.SetCreatedEntity<PositionComponent>(in entity, new(new(i, i)));
                }
                _debugTimer += 4;
            }


            while (_debugResponseQueue.Count > 0 && _debugPositionQueue.Count > 0)
            {
                var entity = _debugResponseQueue.Dequeue();
                var position = _debugPositionQueue.Dequeue();
                entity.Set(new PositionComponent(position));
            }
            _debugTimer -= Globals.GameTime.GetElapsedSeconds();

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
