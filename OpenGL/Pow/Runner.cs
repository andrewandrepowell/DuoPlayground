using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.Core;
using Schedulers;

namespace Pow
{
    internal class Runner : IDisposable
    {
        private readonly World _world;
        private readonly JobScheduler _jobScheduler;
        public Runner()
        {
            Debug.Assert(Globals.State == Globals.States.WaitingForInitPow);
            _world = World.Create();
            _jobScheduler = new(new JobScheduler.Config()
            {
                ThreadPrefixName = "Pow.Thread",
                ThreadCount = 0, // Determined at runtime
                MaxExpectedConcurrentJobs = 64,
                StrictAllocationMode = false,
            });
            World.SharedJobScheduler = _jobScheduler;
        }

        public void Dispose()
        {
            World.Destroy(_world);
            _jobScheduler.Dispose();
        }
    }
}
