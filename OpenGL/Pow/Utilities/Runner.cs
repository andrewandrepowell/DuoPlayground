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
        private readonly CommandBuffer _commandBuffer;
        private readonly Camera _camera;
        private readonly Map _map;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly IRunnerParent _parent;
        private readonly Render _render;
        private readonly InitializeSystem _initializeSystem;
        private readonly DestroySystem _destroySystem;
        private readonly Group<GameTime> _systemGroups;
        private readonly GOCustomSystem _goCustomSystem;
        private readonly GOGeneratorContainer _goGeneratorContainer;
        private readonly AnimationGenerator _animationGenerator;
        private readonly Dictionary<int, EntityTypeNode> _entityTypeNodes = [];
        private bool _initialized;
        private record EntityTypeNode(ComponentType[] ComponentTypes);
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
            _commandBuffer = new();
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
        public void AddEntityType(int id, ComponentType[] componentTypes)
        {
            Debug.Assert(!_initialized);
            Debug.Assert(!_entityTypeNodes.ContainsKey(id));
            Debug.Assert(componentTypes.Contains(typeof(StatusComponent)));
            _entityTypeNodes.Add(id, new(componentTypes));
        }
        public void AddGOCustomManager<T>(int capacity = 32) where T : GOCustomManager, new()
        {
            Debug.Assert(!_initialized);
            _goGeneratorContainer.Add<T>(capacity);
            _goCustomSystem.Add<T>();
            _initializeSystem.Add<GOCustomComponent<T>>();
            _destroySystem.Add<GOCustomComponent<T>>();
        }
        public Entity CreateEntity(int id)
        {
            Debug.Assert(_initialized);
            var entity = _commandBuffer.Create(_entityTypeNodes[id].ComponentTypes);
            _commandBuffer.Set(in entity, new StatusComponent() { State = EntityStates.Initializing });
            return entity; 
        }
#nullable enable
        public void SetCreatedEntity<T>(in Entity entity, in T? value)
        {
            Debug.Assert(_initialized);
            _commandBuffer.Set(in entity, in value);
        }
        public void AddCreatedEntity<T>(in Entity entity, in T? component)
        {
            Debug.Assert(_initialized);
            _commandBuffer.Add(in entity, in component);
        }
#nullable disable
        public void DestroyEntity(in Entity entity)
        {
            Debug.Assert(_initialized);
            ref var statusComponent = ref _world.Get<StatusComponent>(entity);
            Debug.Assert(statusComponent.State == EntityStates.Running);
            statusComponent.State = EntityStates.Destroying;
            _commandBuffer.Destroy(in entity);
        }
        public void Update()
        {
            Debug.Assert(_initialized);
            _destroySystem.Update(Globals.GameTime);
            _commandBuffer.Playback(_world);
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
            _map.Dispose();
        }
    }
}
