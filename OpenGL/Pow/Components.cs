using Pow.Utilities;
using Pow.Utilities.Animations;
using Pow.Utilities.GO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.Core;

namespace Pow.Components
{
    public struct StatusComponent()
    {
        internal EntityStates State = EntityStates.Initializing;
    }
    public readonly struct AnimationComponent : IDisposable
    {
        public readonly AnimationManager Manager;
        public AnimationComponent()
        {
            Manager = Globals.Runner.AnimationGenerator.Acquire();
        }
        public void Dispose()
        {
            Manager.Return();
        }
    }
    public readonly struct GOCustomComponent<T> : IDisposable, IEntityInitialize where T : GOCustomManager
    {
        public readonly GOCustomManager Manager;
        public GOCustomComponent() => Manager = Globals.Runner.GOGeneratorContainer.Acquire<T>();
        public void Initialize(in Entity entity) 
        { 
            Manager.Initialize(entity);
            Manager.Initialize();
        }
        public void Dispose() => Manager.Return();
    }
}
