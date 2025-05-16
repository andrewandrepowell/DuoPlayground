using Arch.Core;
using Arch.System;
using Microsoft.Xna.Framework;
using Pow.Utilities;
using Pow.Components;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.ObjectModel;

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
        private readonly static Directions[] _directions = Enum.GetValues<Directions>();
        private readonly QueryDescription _allAnimationComponents;
        private readonly Dictionary<Layers, ForEach<AnimationComponent>> _drawAnimationComponents;
        private readonly Map _map;
        private readonly Camera _camera;
        private readonly ReadOnlyDictionary<Layers, RenderTarget2D> _pixelArtRenderTargets;
        private readonly RenderTargetBinding[] _prevRenderTargets;
        private readonly Utilities.GameWindow _gameWindow;
        private record LetterBoxNode(RenderTarget2D RenderTarget, Utilities.GameWindow.LetterBoxNode GameWindowNode);
        private readonly ReadOnlyDictionary<Directions, LetterBoxNode> _letterBoxNodes;
        public RenderDrawSystem(
            World world, 
            Map map, 
            Camera camera,
            SizeF gameWindowSize) : base(world)
        {
            var graphicsDevice = Globals.Game.GraphicsDevice;
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
                var pixelArtRenderTargets = new Dictionary<Layers, RenderTarget2D>();
                foreach (var layer in _layers)
                    pixelArtRenderTargets.Add(layer, new RenderTarget2D(
                        graphicsDevice: Globals.Game.GraphicsDevice, 
                        width: (int)gameWindowSize.Width, 
                        height: (int)gameWindowSize.Height,
                        mipMap: false,
                        preferredFormat: SurfaceFormat.Color,
                        preferredDepthFormat: DepthFormat.None,
                        preferredMultiSampleCount: 0,
                        usage: RenderTargetUsage.DiscardContents));
                _pixelArtRenderTargets = new(pixelArtRenderTargets);
            }
            _prevRenderTargets = new RenderTargetBinding[graphicsDevice.RenderTargetCount];
            _gameWindow = new(gameWindowSize);
            {
                var letterBoxNodes = new Dictionary<Directions, LetterBoxNode>();
                foreach (var direction in _directions)
                {
                    var renderTarget = new RenderTarget2D(
                        graphicsDevice: Globals.Game.GraphicsDevice,
                        width: 1,
                        height: 1,
                        mipMap: false,
                        preferredFormat: SurfaceFormat.Color,
                        preferredDepthFormat: DepthFormat.None,
                        preferredMultiSampleCount: 0,
                        usage: RenderTargetUsage.DiscardContents);
                    var colors = new Color[] { Color.Black };
                    renderTarget.SetData(colors);
                    letterBoxNodes.Add(direction, new LetterBoxNode(
                        RenderTarget: renderTarget,
                        GameWindowNode: _gameWindow.LetterBoxNodes[direction]));
                }
                _letterBoxNodes = new(letterBoxNodes);
            }
        }
        public override void Dispose()
        {
            foreach (var renderTarget in _pixelArtRenderTargets.Values)
                renderTarget.Dispose();
            foreach (var letterBoxNode in _letterBoxNodes.Values)
                letterBoxNode.RenderTarget.Dispose();
            base.Dispose();
        }
        public override void Update(in GameTime t)
        {
            ref var view = ref _camera.View;
            var graphicsDevice = Globals.Game.GraphicsDevice;
            var spriteBatch = Globals.SpriteBatch;

            // Draw all layers to either pixel art or smooth art targets.
            graphicsDevice.GetRenderTargets(_prevRenderTargets);
            foreach (var layer in _layers.AsSpan())
            {
                graphicsDevice.SetRenderTarget(_pixelArtRenderTargets[layer]);
                graphicsDevice.Clear(Color.Transparent);

                // Draw map.
                spriteBatch.Begin(transformMatrix: view, samplerState: SamplerState.PointClamp);
                _map.Draw(layer);
                spriteBatch.End();

                // Draw animations.
                spriteBatch.Begin(transformMatrix: view, samplerState: SamplerState.PointClamp);
                World.Query(_allAnimationComponents, _drawAnimationComponents[layer]);
                spriteBatch.End();
            }

            // Update game window related properties, such as game window sizing and letter box sizes.
            _gameWindow.Update();

            // Draw the pixel art targets and smooth art targets to the screen for all layers.
            graphicsDevice.SetRenderTargets(_prevRenderTargets);
            graphicsDevice.Clear(Color.CornflowerBlue);
            foreach (var layer in _layers.AsSpan())
            {
                // Draw pixel art.
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                spriteBatch.Draw(
                    texture: _pixelArtRenderTargets[layer],
                    position: _gameWindow.Offset,
                    sourceRectangle: null,
                    color: Color.White,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: _gameWindow.Scalar,
                    effects: SpriteEffects.None,
                    layerDepth: 0);
                spriteBatch.End();

                // Draw letter boxes if box layer.
                if (layer == Layers.Box)
                {
                    spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                    foreach (var direction in _directions.AsSpan())
                    {
                        var letterBoxNode = _letterBoxNodes[direction];
                        spriteBatch.Draw(
                            texture: letterBoxNode.RenderTarget,
                            destinationRectangle: letterBoxNode.GameWindowNode.DestinationRectangle,
                            sourceRectangle: null,
                            color: Color.White);
                    }
                    spriteBatch.End();
                }
            }
            base.Update(t);
        }
    }
}
