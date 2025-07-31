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
using Pow.Utilities.Gum;
using MonoGameGum;
using System.Diagnostics;

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
            GumService.Default.Update(t);
            World.ParallelQuery(_allAnimationComponents, _updateAnimationComponents);
            base.Update(t);
        }
    }
    internal class RenderDrawSystem : BaseSystem<World, GameTime>
    {
        private readonly static Layers[] _layers = Enum.GetValues<Layers>();
        private readonly static Directions[] _directions = Enum.GetValues<Directions>();
        private readonly static PositionModes[] _gumPositionModes = Enum.GetValues<PositionModes>();
        private readonly QueryDescription _allAnimationComponents;
        private readonly Dictionary<(Layers, PositionModes), ForEach<AnimationComponent>> _drawAnimationComponents;
        private readonly Dictionary<Layers, ForEach<AnimationComponent>> _drawFeatureAnimationComponents;
        private readonly QueryDescription _allGumComponents;
        private readonly Dictionary<Layers, ForEach<GumComponent>> _gumDrawGumComponents;
        private readonly Dictionary<(Layers, PositionModes), ForEach<GumComponent>> _monoDrawGumComponents;
        private readonly Map _map;
        private readonly Camera _camera;
        private readonly ReadOnlyDictionary<Layers, RenderTarget2D> _pixelArtRenderTargets;
        private readonly ReadOnlyDictionary<Layers, RenderTarget2D> _smoothArtRenderTargets;
        private readonly RenderTargetBinding[] _prevRenderTargets;
        private readonly Utilities.GameWindow _gameWindow;
        private readonly Vector2 _windowOrigin;
        private readonly RectangleF _windowScreenBounds;
        private RectangleF _windowMapBounds;
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
                var screenIntersects = 
                _drawAnimationComponents = [];
                foreach (var layer in _layers)
                    foreach (var positionMode in _gumPositionModes)
                    _drawAnimationComponents.Add((layer, positionMode), new((ref AnimationComponent component) =>
                    {
                        var manager = component.Manager;
                        if (manager.Show && 
                            manager.Layer == layer && 
                            manager.PositionMode == positionMode &&
                            ((manager.PositionMode == PositionModes.Screen && _windowScreenBounds.Intersects(manager.Bounds)) ||
                             (manager.PositionMode == PositionModes.Map && _windowMapBounds.Intersects(manager.Bounds))))
                            manager.Draw();
                    }));
                _drawFeatureAnimationComponents = [];
                foreach (var layer in _layers)
                    _drawFeatureAnimationComponents.Add(layer, new((ref AnimationComponent component) =>
                    {
                        var manager = component.Manager;
                        if (manager.Layer == layer && manager.Features.Count > 0)
                        {
                            var spriteBatch = Globals.SpriteBatch;
                            ref var view = ref _camera.View;
                            switch (manager.PositionMode)
                            {
                                case PositionModes.Map:
                                    if (_windowMapBounds.Intersects(manager.Bounds))
                                    {
                                        foreach (var feature in manager.Features)
                                        {
                                            if (!feature.Running) continue;
                                            feature.UpdateEffect();
                                            spriteBatch.Begin(transformMatrix: view, effect: feature.Effect.Effect, samplerState: SamplerState.PointClamp);
                                            manager.Draw();
                                            spriteBatch.End();
                                        }
                                    }
                                    break;
                                case PositionModes.Screen:
                                    if (_windowScreenBounds.Intersects(manager.Bounds))
                                    {
                                        foreach (var feature in manager.Features)
                                        {
                                            if (!feature.Running) continue;
                                            feature.UpdateEffect();
                                            spriteBatch.Begin(effect: feature.Effect.Effect, samplerState: SamplerState.PointClamp);
                                            manager.Draw();
                                            spriteBatch.End();
                                        }
                                    }
                                    break;
                                default:
                                    Debug.Assert(false);
                                    break;
                            }
                        }
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
            {
                var smoothArtRenderTargets = new Dictionary<Layers, RenderTarget2D>();
                foreach (var layer in _layers)
                    smoothArtRenderTargets.Add(layer, new RenderTarget2D(
                        graphicsDevice: Globals.Game.GraphicsDevice,
                        width: (int)gameWindowSize.Width,
                        height: (int)gameWindowSize.Height,
                        mipMap: false,
                        preferredFormat: SurfaceFormat.Color,
                        preferredDepthFormat: DepthFormat.None,
                        preferredMultiSampleCount: 0,
                        usage: RenderTargetUsage.DiscardContents));
                _smoothArtRenderTargets = new(smoothArtRenderTargets);
            }
            _prevRenderTargets = new RenderTargetBinding[graphicsDevice.RenderTargetCount];
            {
                _windowOrigin = new(
                    x: gameWindowSize.Width / 2, 
                    y: gameWindowSize.Height / 2);
                _windowMapBounds = new(
                    x: -_windowOrigin.X, 
                    y: -_windowOrigin.Y,
                    width: gameWindowSize.Width, 
                    height: gameWindowSize.Height);
                _windowScreenBounds = new(
                    x: 0,
                    y: 0,
                    width: gameWindowSize.Width,
                    height: gameWindowSize.Height);
            }
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
            _allGumComponents = new QueryDescription().WithAny<GumComponent>();
            {
                _gumDrawGumComponents = [];
                _monoDrawGumComponents = [];
                foreach (var layer in _layers)
                {
                    _gumDrawGumComponents.Add(layer, new((ref GumComponent component) =>
                    {
                        if (component.Manager.Layer == layer)
                            component.Manager.GumDraw();
                    }));
                    foreach (var positionMode in _gumPositionModes)
                        _monoDrawGumComponents.Add((layer, positionMode), new((ref GumComponent component) =>
                        {
                            if (component.Manager.Layer == layer && component.Manager.PositionMode == positionMode)
                                component.Manager.MonoDraw();
                        }));
                }
            }
        }
        public override void Dispose()
        {
            foreach (var renderTarget in _pixelArtRenderTargets.Values)
                renderTarget.Dispose();
            foreach (var renderTarget in _smoothArtRenderTargets.Values)
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

            // Perform mono draw step of all gum components.
            foreach (var layer in _layers.AsSpan())
                World.Query(_allGumComponents, _gumDrawGumComponents[layer]);

            // Update the window bounds. Used to avoid having to draw animations outside of the camer view.
            _windowMapBounds.Position = _camera.Position - _windowOrigin;

            // Draw all layers to either pixel art or smooth art targets.
            graphicsDevice.GetRenderTargets(_prevRenderTargets);
            foreach (var layer in _layers.AsSpan())
            {
                // pixel art drawing.
                {
                    graphicsDevice.SetRenderTarget(_pixelArtRenderTargets[layer]);
                    graphicsDevice.Clear(Color.Transparent);

                    // Draw map.
                    spriteBatch.Begin(transformMatrix: view, samplerState: SamplerState.PointClamp);
                    _map.Draw(layer);
                    spriteBatch.End();

                    // Draw animations with respect to view matrix.
                    spriteBatch.Begin(transformMatrix: view, samplerState: SamplerState.PointClamp);
                    World.Query(_allAnimationComponents, _drawAnimationComponents[(layer, PositionModes.Map)]);
                    spriteBatch.End();

                    // Draw animations directly to screen.
                    spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                    World.Query(_allAnimationComponents, _drawAnimationComponents[(layer, PositionModes.Screen)]);
                    spriteBatch.End();

                    // Draw features (i.e. shader effects)
                    World.Query(_allAnimationComponents, _drawFeatureAnimationComponents[layer]);
                }

                // smooth art drawing.
                {
                    graphicsDevice.SetRenderTarget(_smoothArtRenderTargets[layer]);
                    graphicsDevice.Clear(Color.Transparent);

                    // Draw gum components with respect to view matrix.
                    spriteBatch.Begin(transformMatrix: view, samplerState: SamplerState.LinearClamp);
                    World.Query(_allGumComponents, _monoDrawGumComponents[(layer, PositionModes.Map)]);
                    spriteBatch.End();

                    // Draw gum components directly to screen.
                    spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
                    World.Query(_allGumComponents, _monoDrawGumComponents[(layer, PositionModes.Screen)]);
                    spriteBatch.End();
                }
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

                // Draw smooth art.
                spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
                spriteBatch.Draw(
                    texture: _smoothArtRenderTargets[layer],
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
