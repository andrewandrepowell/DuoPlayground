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
using System.Text;
using System.Xml.Linq;
using System.Linq;
using Pow.Utilities.Animations;
using nkast.Aether.Physics2D.Dynamics;
using EcsWorld = Arch.Core.World;


namespace OpenGLGame
{
    public enum Maps { LevelDebug0 }
    public enum Animations { BallDebug0, BallDebug1 }
    public enum EntityTypes { Initialize, Wall, Ball }
    public class BallGOManager : GOCustomManager
    {
        private AnimationManager _animationManager;
        private Body _physicsBody;
        private bool _initialized = false;
        public void Initialize(Map.PolygonNode node)
        {
            Debug.Assert(!_initialized);
            {
                _physicsBody = Entity.Get<PhysicsComponent>().Manager.Body;
                _physicsBody.BodyType = BodyType.Dynamic;
                _physicsBody.Mass = 1;
                var position = node.Vertices.Average() + node.Position;
                _physicsBody.CreateCircle(radius: 15, density: 1);
                _physicsBody.Position = position;
            }
            {
                _animationManager = Entity.Get<AnimationComponent>().Manager;
                _animationManager.Play((int)Animations.BallDebug0);
            }
            _initialized = true;
        }
        public override void Update()
        {
            Debug.Assert(_initialized);
            if (!_animationManager.Running)
                _animationManager.Play((int)Animations.BallDebug0);
            _physicsBody.ApplyForce(new Vector2(0, 200000));
            base.Update();
        }
        public override void Cleanup()
        {
            Debug.Assert(_initialized);
            _initialized = false;
            base.Cleanup();
        }
    }
    public class WallGOManager : GOCustomManager
    {
        private bool _initialized = false;
        public void Initialize(Map.PolygonNode node)
        {
            Debug.Assert(!_initialized);
            var body = Entity.Get<PhysicsComponent>().Manager.Body;
            body.BodyType = BodyType.Static;
            body.Mass = 1;
            body.CreatePolygon(vertices: new(node.Vertices), density: 1);
            body.Position = node.Position;
            _initialized = true;
        }
        public override void Cleanup()
        {
            Debug.Assert(false);
            base.Cleanup();
        }
    }
    public class InitializeGOManager : GOCustomManager
    {
        private static InitializeGOManager _manager;
        private readonly Queue<Map.PolygonNode> _polygonNodes = [];
        private readonly Queue<Entity> _polygonEntities = [];
        public static InitializeGOManager Manager => _manager;
        public override void Initialize()
        {
            Debug.Assert(_manager == null);
            _manager = this;
            base.Initialize();
        }
        public override void Update()
        {
            while (_polygonEntities.TryDequeue(out var entity))
            {
                var node = _polygonNodes.Dequeue();
                switch (Enum.Parse<EntityTypes>(node.Parameters["EntityType"]))
                {
                    case EntityTypes.Wall:
                        entity.Get<GOCustomComponent<WallGOManager>>().Manager.Initialize(node);
                        break;
                    case EntityTypes.Ball:
                        entity.Get<GOCustomComponent<BallGOManager>>().Manager.Initialize(node);
                        break;
                    default:
                        Debug.Assert(false); 
                        break;
                }
            }
            base.Update();
        }
        public void Add(Map.PolygonNode node)
        {
            Globals.Runner.CreateEntity((int)Enum.Parse<EntityTypes>(node.Parameters["EntityType"]), _polygonEntities);
            _polygonNodes.Enqueue(node);
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
            runner.AddGOCustomManager<BallGOManager>();
            runner.AddEntityType((int)EntityTypes.Initialize, (EcsWorld world) => world.Create(new StatusComponent(), new GOCustomComponent<InitializeGOManager>()));
            runner.AddEntityType((int)EntityTypes.Wall, (EcsWorld world) => world.Create(new StatusComponent(), new PhysicsComponent(), new GOCustomComponent<WallGOManager>()));
            runner.AddEntityType((int)EntityTypes.Ball, (EcsWorld world) => world.Create(new StatusComponent(), new AnimationComponent(), new PhysicsComponent(), new GOCustomComponent<BallGOManager>()));

        }
        public void Initialize(Map.MapNode node)
        {
            var initialize = InitializeGOManager.Manager;
            var runner = Globals.Runner;
            foreach (ref var polygonNode in node.PolygonNodes.AsSpan())
                initialize.Add(polygonNode);
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Globals.InitializeMonoGame(spriteBatch: _spriteBatch, contentManager: Content);
            Globals.InitializePow(this);

            Globals.Runner.Camera.Zoom = 2f;
            Globals.Runner.Camera.Position = new Vector2(-32, -32);
            Globals.Runner.Camera.Rotation = (float)Math.PI * 0.05f;

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
