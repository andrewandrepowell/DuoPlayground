using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.Bus;
using Arch.Core;
using Microsoft.Xna.Framework.Graphics;
using Pow;
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
            Debug.Assert(Globals.State == Globals.States.WaitingForInitPow);
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

            _parent.Initialize(_map);
            _parent.Initialize(_animationGenerator);


        }
        public Camera Camera => _camera;
        public Map Map => _map;
        public void Update()
        {
            // DEBUG DEBUG DEBUG
            if (!_map.Loaded)
            {
                _camera.Zoom = 1.5f;
                _camera.Rotation = (float)Math.PI * 0.1f;
                _map.Load(0);
            }

            _camera.Size = _graphicsDevice.Viewport.Bounds.Size;
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
        }
    }
}
