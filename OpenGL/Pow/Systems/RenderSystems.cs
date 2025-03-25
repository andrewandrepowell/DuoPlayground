using Arch.Core;
using Arch.System;
using Microsoft.Xna.Framework;
using Pow.Utilities;
using Pow.Components;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Pow.Systems
{
    internal class RenderUpdateSystem : BaseSystem<World, GameTime>
    {
        private readonly QueryDescription _allAnimationComponents;
        private readonly ForEach<AnimationComponent> _updateAnimationComponents;
        public RenderUpdateSystem(World world) : base(world) 
        {
            _allAnimationComponents = new QueryDescription().WithAll<AnimationComponent>();
            _updateAnimationComponents = new((ref AnimationComponent component) => component.Manager.Update());
        }
        public override void Update(in GameTime t)
        {
            World.ParallelQuery(_allAnimationComponents, _updateAnimationComponents);
            base.Update(t);
        }
    }
    internal class RenderDrawSystem : BaseSystem<World, GameTime>
    {
        private readonly static Layers[] _layers = Enum.GetValues<Layers>();
        private readonly QueryDescription _allAnimationComponents;
        private readonly Dictionary<Layers, ForEach<AnimationComponent>> _drawAnimationComponents;
        private readonly SpriteBatch _spriteBatch;
        private readonly Map _map;
        private readonly Camera _camera;
        public RenderDrawSystem(World world, Map map, Camera camera) : base(world)
        {
            _spriteBatch = Globals.SpriteBatch;
            _map = map;
            _camera = camera;
            _allAnimationComponents = new QueryDescription().WithAny<AnimationComponent>();
            _drawAnimationComponents = new();
            foreach (var layer in _layers)
                _drawAnimationComponents.Add(layer, new((ref AnimationComponent component) => 
                {
                    if (component.Manager.Layer == layer) 
                        component.Manager.Draw(); 
                }));
        }
        public override void Update(in GameTime t)
        {
            ref var view = ref _camera.View;
            
            foreach (var layer in _layers.AsSpan())
            {
                _spriteBatch.Begin(transformMatrix: view);
                _map.Draw(layer);
                _spriteBatch.End();

                _spriteBatch.Begin(transformMatrix: view);
                World.Query(_allAnimationComponents, _drawAnimationComponents[layer]);
                _spriteBatch.End();

            }
            
            base.Update(t);
        }
    }
    //internal class Render : IDisposable
    //{
    //    private readonly World _world;
    //    private readonly Camera _camera;
    //    private readonly Map _map;
    //    private readonly RenderUpdateSystem _updateSystem;
    //    private readonly RenderDrawSystem _drawSystem;
    //    public World World => _world;
    //    public Camera Camera => _camera;
    //    public Map Map => _map;
    //    public RenderUpdateSystem UpdateSystem => _updateSystem;
    //    public RenderDrawSystem DrawSystem => _drawSystem;
    //    public Render(World world, Camera camera, Map map)
    //    {
    //        _world = world;
    //        _camera = camera;
    //        _map = map;
    //        _updateSystem = new(this);
    //        _drawSystem = new(this);

    //        _updateSystem.Initialize();
    //        _drawSystem.Initialize();
    //    }
    //    public void Dispose()
    //    {
    //        _updateSystem.Dispose();
    //        _drawSystem.Dispose();
    //    }
    //}
}
