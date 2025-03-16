using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.Core;
using Pow;
using Pow.Systems;
using Schedulers;

namespace Pow.Utilities
{
    public class Runner : IDisposable
    {
        private readonly World _world;
        private readonly JobScheduler _jobScheduler;
        private readonly Camera _camera;
        private readonly Map _map;
        private readonly IRunnerParent _parent;
        private readonly Systems.Render.UpdateDraw _updateDraw;
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

            _camera = new();
            _map = new();

            _parent.Initialize(_map);

            _updateDraw = new(_world, _camera, _map);
        }
        public Camera Camera => _camera;
        public Map Map => _map;
        public void Update()
        {
            // DEBUG DEBUG DEBUG
            if (!_map.Loaded)
                _map.Load(0);

            _updateDraw.UpdateSystem.Update(Globals.GameTime);
        }
        public void Draw()
        {
            _updateDraw.DrawSystem.Update(Globals.GameTime);
        }
        public void Dispose()
        {
            World.Destroy(_world);
            _jobScheduler.Dispose();
            _updateDraw.Dispose();
        }
    }
}
