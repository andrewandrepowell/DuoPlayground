﻿using Pow.Utilities;
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
using Microsoft.Xna.Framework;
using Pow.Utilities.Physics;
using Pow.Utilities.Control;
using Pow.Utilities.Gum;

namespace Pow.Components
{
    public struct StatusComponent()
    {
        internal EntityStates State = EntityStates.Initializing;
    }
    public struct AnimationComponent : IDisposable, IEntityInitialize
    {
        public AnimationManager Manager;
        public void Initialize(in Entity entity) => Manager = Globals.Runner.AnimationGenerator.Acquire();
        public readonly void Dispose() => Manager.Return();
    }
    public struct PhysicsComponent : IDisposable, IEntityInitialize
    {
        public PhysicsManager Manager;
        public void Initialize(in Entity entity) => Manager = Globals.Runner.PhysicsGenerator.Acquire();
        public readonly void Dispose() => Manager.Return();
    }
    public struct ControlComponent : IDisposable, IEntityInitialize
    {
        public ControlManager Manager;
        public void Initialize(in Entity entity) => Manager = Globals.Runner.ControlGenerator.Acquire();
        public readonly void Dispose() => Manager.Return();
    }
    public struct GOCustomComponent<T> : IDisposable, IEntityInitialize where T : GOCustomManager
    {
        public T Manager;
        public void Initialize(in Entity entity)
        {
            Manager = Globals.Runner.GOGeneratorContainer.Acquire<T>();
            Manager.Initialize(entity);
        }
        public readonly void Dispose() => Manager.Return();
    }
    public struct GumComponent : IDisposable, IEntityInitialize
    {
        public GumManager Manager;
        public void Initialize(in Entity entity) => Manager = Globals.Runner.GumGenerator.Acquire();
        public readonly void Dispose() => Manager.Return();
    }
    public record struct PositionComponent(Vector2 Vector);
}
