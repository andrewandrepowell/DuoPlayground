using Pow.Utilities.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.Core;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

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
    public enum Layers 
    { 
        FarSky, MidSky, CloseSky, 
        FarMountain, MidMountain, CloseMountain, 
        FarTree, MidTree, Background, 
        Ground, Protag, Foreground, 
        Interface, InterfaceComponent,
        MenuTransition,
        MenuDimmer, 
        FarMenu, MidMenu, CloseMenu,
        OptionsDimmer,
        FarOptionsMenu, OptionsMenu,
        FarFront, MidFront, CloseFront,
        BranchesTransition,
        BranchesDimmer,
        Box 
    }
    public enum EntityStates { Initializing, Running, Destroying, Destroyed }
    public enum Directions { Left, Right, Up, Down }
    public enum BoxTypes { Collide, Ground, Ceil, Wall, Vault }
    public enum PositionModes { Screen, Map }
    public enum RunningStates { Waiting, Holding, Starting, Running, Stopping }
    public record PolygonNode(Vector2 Position, Vector2[] Vertices, ReadOnlyDictionary<string, string> Parameters);
}
