using Arch.Core;
using Arch.System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collisions.Layers;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Systems
{
    internal class RenderUpdateSystem : BaseSystem<World, GameTime>
    {
        
        private readonly Render _parent;
        public RenderUpdateSystem(Render parent) : base(parent.World) 
        {
            _parent = parent;
        }
        public override void Update(in GameTime t)
        {
          
            var map = _parent.Map;

            map.Update();

            base.Update(t);
        }
    }
    internal class RenderDrawSystem : BaseSystem<World, GameTime>
    {
        private readonly static Layers[] _layers = Enum.GetValues<Layers>();
        private readonly Render _parent;
        private readonly QueryDescription _allSprites;
        public RenderDrawSystem(Render parent) : base(parent.World)
        {
            _parent = parent;
            //_allSprites = new QueryDescription().WithAll<Sprite>();
        }
        public override void Update(in GameTime t)
        {
            ref var view = ref _parent.Camera.View;
            ref var projection = ref _parent.Camera.Projection;
            var map = _parent.Map;

            foreach (ref var layer in _layers.AsSpan())
            {
                map.Draw(in layer, in view, in projection);

            }
            base.Update(t);
        }
    }
    internal class Render : IDisposable
    {
        private readonly World _world;
        private readonly Camera _camera;
        private readonly Map _map;
        private readonly RenderUpdateSystem _updateSystem;
        private readonly RenderDrawSystem _drawSystem;
        public World World => _world;
        public Camera Camera => _camera;
        public Map Map => _map;
        public RenderUpdateSystem UpdateSystem => _updateSystem;
        public RenderDrawSystem DrawSystem => _drawSystem;
        public Render(World world, Camera camera, Map map)
        {
            _world = world;
            _camera = camera;
            _map = map;
            _updateSystem = new(this);
            _drawSystem = new(this);

            _updateSystem.Initialize();
            _drawSystem.Initialize();
        }
        public void Dispose()
        {
            _updateSystem.Dispose();
            _drawSystem.Dispose();
        }
    }
}
