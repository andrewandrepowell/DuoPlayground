using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Arch.Buffer;
using Arch.Core;
using Arch.System;
using Arch.Core.Extensions;
using Arch.Core.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pow.Systems;
using Pow.Utilities.Animations;
using Pow.Utilities.GO;
using Pow.Components;
using Schedulers;
using System.Reflection;

namespace Pow.Utilities
{
    public interface IRunnerParent
    {
        public void Initialize(Runner runner);
    }
    public class Runner : IDisposable, IMapParent
    {
        private readonly World _world;
        private readonly JobScheduler _jobScheduler;
        private readonly Camera _camera;
        private readonly Map _map;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly IRunnerParent _parent;
        private readonly Queue<CreateEntityNode> _createEntities = [];
        private readonly Queue<DestroyEntityNode> _destroyEntities = [];
        private readonly Render _render;
        private readonly InitializeSystem _initializeSystem;
        private readonly DestroySystem _destroySystem;
        private readonly Group<GameTime> _systemGroups;
        private readonly GOCustomSystem _goCustomSystem;
        private readonly GOGeneratorContainer _goGeneratorContainer;
        private readonly AnimationGenerator _animationGenerator;
        private readonly Dictionary<int, EntityTypeNode> _entityTypeNodes = [];
        private bool _initialized;
        private record EntityTypeNode(Func<World, Entity> CreateEntity);
        private readonly record struct CreateEntityNode(EntityTypeNode EntityTypeNode, Queue<Entity> ResponseQueue = null);
        private readonly record struct DestroyEntityNode(Entity Entity);
        private unsafe void ServiceCreateEntities()
        {
            while (_createEntities.TryDequeue(out var node))
            {
                var entity = node.EntityTypeNode.CreateEntity(_world);
                Debug.Assert(_world.Has<StatusComponent>(entity));
                if (node.ResponseQueue != null) node.ResponseQueue.Enqueue(entity);
            }
        }
        private void ServiceDestroyEntities()
        {
            while (_destroyEntities.TryDequeue(out var node))
                _world.Destroy(node.Entity);
        }
        public Runner(IRunnerParent parent)
        {
            // Create objects.
            _initialized = false;
            _parent = parent;
            _world = World.Create();
            _jobScheduler = new(new JobScheduler.Config()
            {
                ThreadPrefixName = "Pow.Thread",
                ThreadCount = 0, // Determined at runtime
                MaxExpectedConcurrentJobs = 64,
                StrictAllocationMode = false,
            });
            World.SharedJobScheduler = _jobScheduler;
            _graphicsDevice = Globals.SpriteBatch.GraphicsDevice;
            _camera = new();
            _map = new(this);
            _render = new(_world, _camera, _map);
            _goCustomSystem = new(_world);
            _initializeSystem = new InitializeSystem(_world);
            _destroySystem = new DestroySystem(_world);
            _systemGroups = new Group<GameTime>(
                "Systems",
                _goCustomSystem,
                new PositionSystem(_world),
                _render.UpdateSystem);
            _animationGenerator = new();
            _goGeneratorContainer = new();

            // Associate components with initialize and destroy systems.
            _initializeSystem.Add<AnimationComponent>();
            _initializeSystem.Add<InitializeComponent>();
            _destroySystem.Add<AnimationComponent>();

            // Let the parent initialize.
            _parent.Initialize(this);

            // Initialize based on parent configurations.
            _animationGenerator.Initialize();
            _goGeneratorContainer.Initialize();

            _initialized = true;
        }
        public void Initialize(Map.MapNode node)
        {

        }
        public Camera Camera => _camera;
        public Map Map => _map;
        public AnimationGenerator AnimationGenerator => _animationGenerator;
        internal GOGeneratorContainer GOGeneratorContainer => _goGeneratorContainer;
        public void AddEntityType(int id, Func<World, Entity> createEntity)
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
        public unsafe void CreateEntity(int id, Queue<Entity> responseQueue = null)
        {
            Debug.Assert(_initialized);
            Debug.Assert(_entityTypeNodes.ContainsKey(id));
            _createEntities.Enqueue(new(_entityTypeNodes[id], responseQueue));
        }
        public void DestroyEntity(in Entity entity)
        {
            Debug.Assert(_initialized);
            ref var statusComponent = ref _world.Get<StatusComponent>(entity);
            Debug.Assert(statusComponent.State == EntityStates.Running);
            statusComponent.State = EntityStates.Destroying;
            _destroyEntities.Enqueue(new(entity));
        }
        public void Update()
        {
            Debug.Assert(_initialized);
            _destroySystem.Update(Globals.GameTime);
            ServiceDestroyEntities();
            ServiceCreateEntities();
            _initializeSystem.Update(Globals.GameTime);
            _systemGroups.Update(Globals.GameTime);
        }
        public void Draw()
        {
            Debug.Assert(_initialized);
            _render.DrawSystem.Update(Globals.GameTime);
        }
        public void Dispose()
        {
            Debug.Assert(_initialized);
            World.Destroy(_world);
            _jobScheduler.Dispose();
            _render.Dispose();
            _initializeSystem.Dispose();
            _destroySystem.Dispose();
            _systemGroups.Dispose();
            _map.Dispose();
        }
    }
}
