using Pow.Utilities;
using Pow.Utilities.Animations;
using Pow.Utilities.GO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.Core;
using Arch.Core.Utils;
using System.Diagnostics;
using Arch.Core.Extensions;
using System.Numerics;

namespace Pow.Components
{
    public delegate void EntityAction(in Entity entity);

    public struct StatusComponent()
    {
        internal EntityStates State = EntityStates.Initializing;
    }
    public struct AnimationComponent : IDisposable, IEntityInitialize
    {
        public AnimationManager Manager;
        public void Initialize(in Entity entity) => Manager = Globals.Runner.AnimationGenerator.Acquire();
        public void Dispose() => Manager.Return();
    }
    public struct GOCustomComponent<T> : IDisposable, IEntityInitialize where T : GOCustomManager
    {
        public GOCustomManager Manager;
        public void Initialize(in Entity entity)
        {
            Manager = Globals.Runner.GOGeneratorContainer.Acquire<T>();
            Manager.Initialize(entity);
        }
        public void Dispose() => Manager.Return();
    }
    public record struct PositionComponent(Vector2 Vector);
    
    public record struct InitializeComponent(EntityAction Action) : IEntityInitialize
    {
        public void Initialize(in Entity entity) => Action(in entity);
    }
}
