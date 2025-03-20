using System;
using Arch.Core;
using Microsoft.Xna.Framework.Graphics;
using Pow.Systems;
using Pow.Utilities.Animations;
using Schedulers;

namespace Pow.Utilities
{
    public class Runner : IDisposable
    {
        private readonly World _world;
        private readonly JobScheduler _jobScheduler;
        private readonly Camera _camera;
        private readonly Map _map;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly IRunnerParent _parent;
        private readonly Render _render;
        private readonly AnimationGenerator _animationGenerator;
        public Runner(IRunnerParent parent)
        {
            // Create objects.
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
            _map = new();
            _render = new(_world, _camera, _map);
            _animationGenerator = new();

            // Let the parent initialize.
            _parent.Initialize(this);

            // Initialize based on paret configurations.
            _animationGenerator.Initialize();
        }
        public Camera Camera => _camera;
        public Map Map => _map;
        public AnimationGenerator AnimationGenerator => _animationGenerator;
        public void Update()
        {
            _render.UpdateSystem.Update(Globals.GameTime);
        }
        public void Draw()
        {
            _render.DrawSystem.Update(Globals.GameTime);
        }
        public void Dispose()
        {
            World.Destroy(_world);
            _jobScheduler.Dispose();
            _render.Dispose();
            _map.Dispose();
        }
    }
}
