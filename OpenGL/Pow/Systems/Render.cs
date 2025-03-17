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

namespace Pow.Systems.Render
{
    internal class UpdateSystem : BaseSystem<World, GameTime>
    {
        
        private readonly UpdateDraw _parent;
        public UpdateSystem(UpdateDraw parent) : base(parent.World) 
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
    internal class DrawSystem : BaseSystem<World, GameTime>
    {
        private readonly static Layers[] _layers = Enum.GetValues<Layers>();
        private readonly UpdateDraw _parent;
        private readonly QueryDescription _allSprites;
        public DrawSystem(UpdateDraw parent) : base(parent.World)
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
    internal class UpdateDraw : IDisposable
    {
        private readonly World _world;
        private readonly Camera _camera;
        private readonly Map _map;
        private readonly UpdateSystem _updateSystem;
        private readonly DrawSystem _drawSystem;
        public World World => _world;
        public Camera Camera => _camera;
        public Map Map => _map;
        public UpdateSystem UpdateSystem => _updateSystem;
        public DrawSystem DrawSystem => _drawSystem;
        public UpdateDraw(World world, Camera camera, Map map)
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
