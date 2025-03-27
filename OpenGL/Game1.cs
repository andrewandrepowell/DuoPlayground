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
using System.Diagnostics;


namespace OpenGLGame
{
    public enum Maps { LevelDebug0 }
    public enum Animations { BallDebug0, BallDebug1 }
    public enum EntityTypes { Initialize, Wall }
    public class WallGOManager : GOCustomManager
    {
        private bool _initialized = false;
        public void Initialize(Map.PolygonNode node)
        {
            Debug.Assert(!_initialized);
            var body = Entity.Get<PhysicsComponent>().Manager.Body;
            body.BodyType = nkast.Aether.Physics2D.Dynamics.BodyType.Static;
            body.CreatePolygon(vertices: new(node.Vertices), density: 1);
            body.Position = node.Position;
            _initialized = true;
        }
    }
    public class InitializeGOManager : GOCustomManager
    {
        private static InitializeGOManager _manager;
        private readonly Queue<Map.PolygonNode> _polygonNodes = [];
        private readonly Queue<Entity> _entities = [];
        public Queue<Map.PolygonNode> PolygonNodes => _polygonNodes;
        public Queue<Entity> Entities => _entities;
        public static InitializeGOManager Manager => _manager;
        public override void Initialize()
        {
            Debug.Assert(_manager == null);
            _manager = this;
            base.Initialize();
        }
        public override void Update()
        {
            while (_entities.TryDequeue(out var entity))
            {
                if (entity.Has<GOCustomComponent<WallGOManager>>())
                {
                    var manager = entity.Get<GOCustomComponent<WallGOManager>>().Manager;
                    var polygonNode = _polygonNodes.Dequeue();
                    manager.Initialize(polygonNode);
                }
            }
            base.Update();
        }
    }
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
            runner.Map.Configure((int)Maps.LevelDebug0, "tiled/test_map_0");
            runner.AnimationGenerator.ConfigureSprite(0, "images/test_ball_0", new(32, 32));
            runner.AnimationGenerator.ConfigureAnimation((int)Animations.BallDebug0, 0, 0, [0, 1, 2, 3], 0.25f, false);
            runner.AnimationGenerator.ConfigureAnimation((int)Animations.BallDebug1, 0, 1, [0, 4, 5], 0.25f, false);
            runner.AddGOCustomManager<InitializeGOManager>();
            runner.AddGOCustomManager<WallGOManager>();
            runner.AddEntityType((int)EntityTypes.Initialize, (World world) => world.Create(new StatusComponent(), new GOCustomComponent<InitializeGOManager>()));
            runner.AddEntityType((int)EntityTypes.Wall, (World world) => world.Create(new StatusComponent(), new PhysicsComponent(), new GOCustomComponent<WallGOManager>()));
        }
        public void Initialize(Map.MapNode node)
        {
            var initialize = InitializeGOManager.Manager;
            var runner = Globals.Runner;
            foreach (var polygonNode in node.PolygonNodes)
            {
                runner.CreateEntity((int)EntityTypes.Wall, initialize.Entities);
                initialize.PolygonNodes.Enqueue(polygonNode);
            }
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Globals.InitializeMonoGame(spriteBatch: _spriteBatch, contentManager: Content);
            Globals.InitializePow(this);

            Globals.Runner.Camera.Zoom = 2f;
            //Globals.Runner.Camera.Position = new Vector2(-32, -32);
            //Globals.Runner.Camera.Rotation = (float)Math.PI * 0.05f;

            Globals.Runner.CreateEntity((int)EntityTypes.Initialize);
            Globals.Runner.Map.Load((int)Maps.LevelDebug0);
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
