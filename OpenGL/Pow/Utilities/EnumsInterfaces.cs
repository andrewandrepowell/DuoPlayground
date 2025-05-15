using Pow.Utilities.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.Core;

namespace Pow.Utilities
{
    public interface IUpdate
    {
        void Update();
    }
    public interface IEntityInitialize
    {
        void Initialize(in Entity entity);
    }
    public enum Layers { Background, Ground, Foreground, Interface, Dimmer, Menu, Box }
    public enum EntityStates { Initializing, Running, Destroying, Destroyed }
    public enum Directions { Left, Right, Up, Down }
}
