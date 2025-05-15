using System;
using System.Collections.Generic;
using System.Diagnostics;
using Arch.Core;
using Arch.System;
using Microsoft.Xna.Framework;
using Pow.Systems;
using Pow.Utilities.Animations;
using Pow.Utilities.GO;
using Pow.Components;
using Schedulers;
using EcsWorld = Arch.Core.World;
using PhysicsWorld = nkast.Aether.Physics2D.Dynamics.World;
using Pow.Utilities.Physics;
using Pow.Utilities.Control;
using MonoGame.Extended;

namespace Pow.Utilities
{
    public interface IRunnerParent
    {
        public SizeF GameWindowSize { get; }
        public void Initialize(Runner runner);
        public void Initialize(Map.MapNode node);
    }
    public class Runner : IDisposable, IMapParent
    {
        private readonly EcsWorld _ecsWorld;
        private readonly PhysicsWorld _physicsWorld;
        private readonly JobScheduler _jobScheduler;
        private readonly Camera _camera;
        private readonly Map _map;
        private readonly IRunnerParent _parent;
        private readonly Queue<CreateEntityNode> _createEntities = [];
        private readonly Queue<DestroyEntityNode> _destroyEntities = [];
        private readonly InitializeSystem _initializeSystem;
        private readonly DestroySystem _destroySystem;
        private readonly RenderDrawSystem _renderDrawSystem;
        private readonly Group<GameTime> _systemGroups;
        private readonly GOCustomSystem _goCustomSystem;
        private readonly GOGeneratorContainer _goGeneratorContainer;
        private readonly AnimationGenerator _animationGenerator;
        private readonly PhysicsGenerator _physicsGenerator;
        private readonly ControlGenerator _controlGenerator;
        private readonly Dictionary<int, EntityTypeNode> _entityTypeNodes = [];
        private bool _initialized;
        private record EntityTypeNode(Func<EcsWorld, Entity> CreateEntity);
        private readonly record struct CreateEntityNode(EntityTypeNode EntityTypeNode, Queue<Entity> ResponseQueue = null);
        private readonly record struct DestroyEntityNode(Entity Entity);
        private void ServiceCreateEntities()
        {
            while (_createEntities.TryDequeue(out var node))
            {
                var entity = node.EntityTypeNode.CreateEntity(_ecsWorld);
                Debug.Assert(_ecsWorld.Has<StatusComponent>(entity));
                node.ResponseQueue?.Enqueue(entity);
            }
        }
        private void ServiceDestroyEntities()
        {
            while (_destroyEntities.TryDequeue(out var node))
                _ecsWorld.Destroy(node.Entity);
        }
        public Runner(IRunnerParent parent)
        {
            // Create objects.
            _initialized = false;
            _parent = parent;
            _physicsWorld = new PhysicsWorld(Vector2.Zero);
            _ecsWorld = EcsWorld.Create();
            _jobScheduler = new(new JobScheduler.Config()
            {
                ThreadPrefixName = "Pow.Thread",
                ThreadCount = 0, // Determined at runtime
                MaxExpectedConcurrentJobs = 64,
                StrictAllocationMode = false,
            });
            EcsWorld.SharedJobScheduler = _jobScheduler;
            _camera = new();
            _map = new(this);
            _renderDrawSystem = new(
                world: _ecsWorld, 
                map: _map, 
                camera: _camera, 
                gameWindowSize: parent.GameWindowSize);
            _goCustomSystem = new(_ecsWorld);
            _initializeSystem = new InitializeSystem(_ecsWorld);
            _destroySystem = new DestroySystem(_ecsWorld);
            _systemGroups = new Group<GameTime>(
                "Systems",
                _goCustomSystem,
                new ControlSystem(_ecsWorld),
                new PhysicsSystem(_ecsWorld, _physicsWorld),
                new PositionSystem(_ecsWorld),
                new RenderUpdateSystem(_ecsWorld));
            _animationGenerator = new();
            _goGeneratorContainer = new();
            _physicsGenerator = new(_physicsWorld);
            _controlGenerator = new();

            // Associate components with initialize and destroy systems.
            _initializeSystem.Add<AnimationComponent>();
            _initializeSystem.Add<PhysicsComponent>();
            _initializeSystem.Add<ControlComponent>();
            _destroySystem.Add<AnimationComponent>();
            _destroySystem.Add<PhysicsComponent>();
            _destroySystem.Add<ControlComponent>();

            // Let the parent initialize.
            _parent.Initialize(this);

            // Initialize based on parent configurations.
            _initializeSystem.Initialize();
            _destroySystem.Initialize();
            _systemGroups.Initialize();
            _animationGenerator.Initialize();
            _goGeneratorContainer.Initialize();
            _physicsGenerator.Initialize();
            _controlGenerator.Initialize();

            _initialized = true;
        }
        public void Initialize(Map.MapNode node) => _parent.Initialize(node);
        public Camera Camera => _camera;
        public Map Map => _map;
        public AnimationGenerator AnimationGenerator => _animationGenerator;
        public PhysicsGenerator PhysicsGenerator => _physicsGenerator;
        public ControlGenerator ControlGenerator => _controlGenerator;
        internal GOGeneratorContainer GOGeneratorContainer => _goGeneratorContainer;
        public void AddEntityType(int id, Func<EcsWorld, Entity> createEntity)
        {
            Debug.Assert(!_initialized);
            Debug.Assert(!_entityTypeNodes.ContainsKey(id));
            _entityTypeNodes.Add(id, new(createEntity));
        }
        public void AddGOCustomManager<T>(int capacity = 32) where T : GOCustomManager, new()
        {
            Debug.Assert(!_initialized);
            _goGeneratorContainer.Add<T>(capacity);
            _goCustomSystem.Add<T>();
            _initializeSystem.Add<GOCustomComponent<T>>();
            _destroySystem.Add<GOCustomComponent<T>>();
        }
        public void InitializeGameWindow(Size size)
        {
            Debug.Assert(!_initialized);
        }
        public unsafe void CreateEntity(int id, Queue<Entity> responseQueue = null)
        {
            Debug.Assert(_initialized);
            Debug.Assert(_entityTypeNodes.ContainsKey(id));
            _createEntities.Enqueue(new(_entityTypeNodes[id], responseQueue));
        }
        public void DestroyEntity(in Entity entity)
        {
            Debug.Assert(_initialized);
            ref var statusComponent = ref _ecsWorld.Get<StatusComponent>(entity);
            Debug.Assert(statusComponent.State == EntityStates.Running);
            statusComponent.State = EntityStates.Destroying;
            _destroyEntities.Enqueue(new(entity));
        }
        public void Update()
        {
            Debug.Assert(_initialized);
            {
                ServiceCreateEntities();
                _initializeSystem.Update(Globals.GameTime);
            }
            _systemGroups.Update(Globals.GameTime);
            {
                _destroySystem.Update(Globals.GameTime);
                ServiceDestroyEntities();
            }
            _map.Update();
        }
        public void Draw()
        {
            Debug.Assert(_initialized);
            _renderDrawSystem.Update(Globals.GameTime);
        }
        public void Dispose()
        {
            Debug.Assert(_initialized);
            EcsWorld.Destroy(_ecsWorld);
            _jobScheduler.Dispose();
            _renderDrawSystem.Dispose();
            _initializeSystem.Dispose();
            _destroySystem.Dispose();
            _systemGroups.Dispose();
            _map.Dispose();
        }
    }
}
