using Arch.Core;
using Arch.System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collisions.Layers;
using Pow.Utilities;
using Pow.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace Pow.Systems
{
    internal class RenderUpdateSystem : BaseSystem<World, GameTime>
    {
        
        private readonly Render _parent;
        private readonly QueryDescription _allAnimationComponents;
        private readonly ForEach<AnimationComponent> _updateAnimationComponents;
        public RenderUpdateSystem(Render parent) : base(parent.World) 
        {
            _parent = parent;
            _allAnimationComponents = new QueryDescription().WithAll<AnimationComponent>();
            _updateAnimationComponents = new((ref AnimationComponent component) => component.Manager.Update());
        }
        public override void Update(in GameTime t)
        {
            World.Query(_allAnimationComponents, _updateAnimationComponents);
            base.Update(t);
        }
    }
    internal class RenderDrawSystem : BaseSystem<World, GameTime>
    {
        private readonly static Layers[] _layers = Enum.GetValues<Layers>();
        private readonly Render _parent;
        private readonly QueryDescription _allAnimationComponents;
        private readonly Dictionary<Layers, ForEach<AnimationComponent>> _updateAnimationComponents;
        public RenderDrawSystem(Render parent) : base(parent.World)
        {
            _parent = parent;
            _allAnimationComponents = new QueryDescription().WithAll<AnimationComponent>();
            _updateAnimationComponents = new();
            foreach (var layer in _layers)
            {
                _updateAnimationComponents.Add(layer, new((ref AnimationComponent component) =>
                {
                    if (layer == component.Manager.Layer)
                        component.Manager.Draw();
                }));
            }
        }
        public override void Update(in GameTime t)
        {
            ref var view = ref _parent.Camera.View;
            var spriteBatch = Globals.SpriteBatch;
            var map = _parent.Map;

            
            foreach (var layer in _layers.AsSpan())
            {
                spriteBatch.Begin(transformMatrix: view);
                map.Draw(layer);
                spriteBatch.End();

                spriteBatch.Begin(transformMatrix: view);
                World.Query(_allAnimationComponents, _updateAnimationComponents[layer]);
                spriteBatch.End();

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
