using Arch.Core;
using Arch.System;
using Microsoft.Xna.Framework;
using Pow.Utilities;
using Pow.Components;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

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
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Map _map;
        private readonly Camera _camera;
        private readonly Dictionary<Layers, RenderTarget2D> _pixelArtRenderTargets;
        private readonly RenderTargetBinding[] _prevRenderTargets;
        private readonly Utilities.GameWindow _gameWindow;
        public RenderDrawSystem(
            World world, 
            Map map, 
            Camera camera,
            SizeF gameWindowSize) : base(world)
        {
            _spriteBatch = Globals.SpriteBatch;
            _graphicsDevice = Globals.Game.GraphicsDevice;
            _map = map;
            _camera = camera;
            _allAnimationComponents = new QueryDescription().WithAny<AnimationComponent>();
            {
                _drawAnimationComponents = [];
                foreach (var layer in _layers)
                    _drawAnimationComponents.Add(layer, new((ref AnimationComponent component) =>
                    {
                        if (component.Manager.Layer == layer)
                            component.Manager.Draw();
                    }));
            }
            {
                _pixelArtRenderTargets = [];
                foreach (var layer in _layers)
                    _pixelArtRenderTargets.Add(layer, new RenderTarget2D(
                        graphicsDevice: Globals.Game.GraphicsDevice, 
                        width: (int)gameWindowSize.Width, 
                        height: (int)gameWindowSize.Height,
                        mipMap: false,
                        preferredFormat: SurfaceFormat.Color,
                        preferredDepthFormat: DepthFormat.None,
                        preferredMultiSampleCount: 0,
                        usage: RenderTargetUsage.DiscardContents));
            }
            _prevRenderTargets = new RenderTargetBinding[_graphicsDevice.RenderTargetCount];
            _gameWindow = new(gameWindowSize);
        }
        public override void Dispose()
        {
            foreach (var renderTarget in _pixelArtRenderTargets.Values)
                renderTarget.Dispose();
            base.Dispose();
        }
        public override void Update(in GameTime t)
        {
            ref var view = ref _camera.View;

            _graphicsDevice.GetRenderTargets(_prevRenderTargets);
            foreach (var layer in _layers.AsSpan())
            {
                _graphicsDevice.SetRenderTarget(_pixelArtRenderTargets[layer]);
                _graphicsDevice.Clear(Color.Transparent);

                _spriteBatch.Begin(transformMatrix: view, samplerState: SamplerState.PointClamp);
                _map.Draw(layer);
                _spriteBatch.End();

                _spriteBatch.Begin(transformMatrix: view, samplerState: SamplerState.PointClamp);
                World.Query(_allAnimationComponents, _drawAnimationComponents[layer]);
                _spriteBatch.End();
            }

            _gameWindow.Update();

            _graphicsDevice.SetRenderTargets(_prevRenderTargets);
            _graphicsDevice.Clear(Color.CornflowerBlue);
            foreach (var layer in _layers.AsSpan())
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _spriteBatch.Draw(
                    texture: _pixelArtRenderTargets[layer],
                    position: _gameWindow.Offset,
                    sourceRectangle: null,
                    color: Color.White,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: _gameWindow.Scalar,
                    effects: SpriteEffects.None,
                    layerDepth: 0);
                _spriteBatch.End();
            }
            base.Update(t);
        }
    }
}
