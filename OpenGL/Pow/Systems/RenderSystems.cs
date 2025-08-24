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
        private readonly QueryDescription _allParticleEffectComponents;
        private readonly ForEach<ParticleEffectComponent> _updateParticleEffectComponents;
        public RenderUpdateSystem(World world) : base(world) 
        {
            _allAnimationComponents = new QueryDescription().WithAll<AnimationComponent>();
            _updateParticleEffectComponents = new((ref ParticleEffectComponent component) => component.Manager.Update());
            _allParticleEffectComponents = new QueryDescription().WithAll<ParticleEffectComponent>();
            _updateAnimationComponents = new((ref AnimationComponent component) => component.Manager.Update());
        }
        public override void Update(in GameTime t)
        {
            GumService.Default.Update(t);
            World.ParallelQuery(_allParticleEffectComponents, _updateParticleEffectComponents);
            World.ParallelQuery(_allAnimationComponents, _updateAnimationComponents);
            base.Update(t);
        }
    }
    internal class RenderDrawSystem : BaseSystem<World, GameTime>
    {
        private readonly static Layers[] _layers = Enum.GetValues<Layers>();
        private readonly static Directions[] _directions = Enum.GetValues<Directions>();
        private readonly static PositionModes[] _positionModes = Enum.GetValues<PositionModes>();
        private readonly QueryDescription _allParticleEffectComponents;
        private readonly Dictionary<(Layers, PositionModes), ForEach<ParticleEffectComponent>> _drawParticleEffectComponents;
        private readonly QueryDescription _allAnimationComponents;
        private readonly Dictionary<(Layers, PositionModes), ForEach<AnimationComponent>> _drawAnimationComponents;
        private readonly Dictionary<Layers, ForEach<AnimationComponent>> _drawFeatureAnimationComponents;
        private readonly QueryDescription _allGumComponents;
        private readonly Dictionary<Layers, ForEach<GumComponent>> _gumDrawGumComponents;
        private readonly Dictionary<(Layers, PositionModes), ForEach<GumComponent>> _monoDrawGumComponents;
        private readonly Map _map;
        private readonly Camera _camera;
        private readonly ReadOnlyDictionary<Layers, RenderTarget2D> _pixelArtRenderTargets;
        private readonly RenderTargetBinding[] _prevRenderTargets;
        private readonly Utilities.GameWindow _gameWindow;
        private readonly Vector2 _windowOrigin;
        private readonly RectangleF _windowScreenBounds;
        private RectangleF _windowMapBounds;
        private readonly Dictionary<PositionModes, Matrix> _viewMatrices;
        private readonly Dictionary<PositionModes, Matrix> _viewProjectionMatrices;
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
            _viewMatrices = new()
            {
                { PositionModes.Map, Matrix.Identity },
                { PositionModes.Screen, Matrix.Identity },
            };
            _viewProjectionMatrices = new()
            {
                { PositionModes.Map, _camera.Projection },
                { PositionModes.Screen, _camera.Projection },
            };
            _allParticleEffectComponents = new QueryDescription().WithAll<ParticleEffectComponent>();
            {
                _drawParticleEffectComponents = [];
                foreach (var layer in _layers)
                    foreach (var positionMode in _positionModes)
                        _drawParticleEffectComponents.Add((layer, positionMode), new((ref ParticleEffectComponent component) =>
                        {
                            var manager = component.Manager;
                            if (manager.Show &&
                                manager.Layer == layer &&
                                manager.PositionMode == positionMode)
                                manager.Draw();
                        }));
            }
            _allAnimationComponents = new QueryDescription().WithAll<AnimationComponent>();
            {
                _drawAnimationComponents = [];
                foreach (var layer in _layers)
                    foreach (var positionMode in _positionModes)
                    _drawAnimationComponents.Add((layer, positionMode), new((ref AnimationComponent component) =>
                    {
                        var manager = component.Manager;
                        if (manager.Show && 
                            manager.Layer == layer &&
                            manager.Visibility > 0 &&
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
                        if (manager.Layer == layer && 
                            manager.Features.Count > 0 &&
                            manager.Visibility > 0 &&
                            ((manager.PositionMode == PositionModes.Screen && _windowScreenBounds.Intersects(manager.Bounds)) ||
                             (manager.PositionMode == PositionModes.Map && _windowMapBounds.Intersects(manager.Bounds))))
                        {
                            var spriteBatch = Globals.SpriteBatch;
                            foreach (var feature in manager.Features)
                            {
                                if (!feature.Show) continue;
                                feature.UpdateEffect(viewProjection: _viewProjectionMatrices[manager.PositionMode]);
                                spriteBatch.Begin(transformMatrix: _viewMatrices[manager.PositionMode], effect: feature.Effect.Effect, samplerState: SamplerState.PointClamp);
                                manager.Draw();
                                spriteBatch.End();
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
                    foreach (var positionMode in _positionModes)
                        _monoDrawGumComponents.Add((layer, positionMode), new((ref GumComponent component) =>
                        {
                            if (component.Manager.Show && component.Manager.Layer == layer && component.Manager.PositionMode == positionMode)
                                component.Manager.MonoDraw();
                        }));
                }
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
            var graphicsDevice = Globals.Game.GraphicsDevice;
            var spriteBatch = Globals.SpriteBatch;

            // Configure view and view projection matrices.
            _viewMatrices[PositionModes.Map] = _camera.View;
            _viewProjectionMatrices[PositionModes.Screen] = _camera.Projection;
            _viewProjectionMatrices[PositionModes.Map] = _camera.ViewProjection;

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
                    spriteBatch.Begin(transformMatrix: _viewMatrices[PositionModes.Map], samplerState: SamplerState.PointClamp);
                    _map.Draw(layer);
                    spriteBatch.End();

                    // Draw animations.
                    foreach (var positionMode in _positionModes.AsSpan())
                    {
                        spriteBatch.Begin(transformMatrix: _viewMatrices[positionMode], samplerState: SamplerState.PointClamp);
                        World.Query(_allAnimationComponents, _drawAnimationComponents[(layer, positionMode)]);
                        spriteBatch.End();
                    }

                    // Draw features (i.e. shader effects)
                    World.Query(_allAnimationComponents, _drawFeatureAnimationComponents[layer]);

                    // Draw particle effects
                    foreach (var positionMode in _positionModes.AsSpan())
                    {
                        spriteBatch.Begin(transformMatrix: _viewMatrices[positionMode], samplerState: SamplerState.PointClamp);
                        World.Query(_allParticleEffectComponents, _drawParticleEffectComponents[(layer, positionMode)]);
                        spriteBatch.End();
                    }

                    // Draw gum components.
                    foreach (var positionMode in _positionModes.AsSpan())
                    {
                        spriteBatch.Begin(transformMatrix: _viewMatrices[positionMode], samplerState: SamplerState.PointClamp);
                        World.Query(_allGumComponents, _monoDrawGumComponents[(layer, positionMode)]);
                        spriteBatch.End();
                    }
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
