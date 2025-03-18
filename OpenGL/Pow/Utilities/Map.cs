using System;
using System.Collections.Generic;
using System.Linq;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;

namespace Pow.Utilities
{
    public class Map : IDisposable
    {
        private readonly Dictionary<int, ConfigNode> _configNodes = [];
        private readonly Dictionary<int, MapNode> _mapNodes = [];
        private MapNode _mapNode;
        private bool _loaded = false;
        private record ConfigNode(string AssetName);
        private record MapNode(ConfigNode Config, TiledMap Map, ReadOnlyDictionary<Layers, RenderTarget2D[]> RenderTargets);
        private RenderTarget2D DrawMapLayerToRenderTarget(TiledMap map, TiledMapRenderer renderer, TiledMapLayer mapLayer)
        {
            var graphicsDevice = Globals.SpriteBatch.GraphicsDevice;
            var renderTarget = new RenderTarget2D(graphicsDevice: graphicsDevice, width: map.WidthInPixels, height: map.HeightInPixels);
            var previousTarget = graphicsDevice.GetRenderTargets();
            graphicsDevice.SetRenderTargets(renderTarget);
            graphicsDevice.Clear(Color.Transparent);
            renderer.Draw(mapLayer);
            graphicsDevice.SetRenderTargets(previousTarget);
            return renderTarget;
        }
        public bool Loaded => _loaded;
        public void Configure(int id, string assetName)
        {
            Debug.Assert(!_loaded);
            Debug.Assert(!_configNodes.ContainsKey(id));
            _configNodes.Add(id, new(assetName));
        }
        public void Load(int id)
        {
            Debug.Assert(!_loaded);
            if (!_mapNodes.ContainsKey(id))
            {
                var configNode = _configNodes[id];
                var map = Globals.ContentManager.Load<TiledMap>(configNode.AssetName);
                var renderer = new TiledMapRenderer(Globals.SpriteBatch.GraphicsDevice, map);
                var renderTargets = Enum
                    .GetValues<Layers>()
                    .Select(layer => (layer, map.Layers
                        .Where(mapLayer => mapLayer.Name.StartsWith(layer.ToString().ToLower()))
                        .Select(mapLayer => DrawMapLayerToRenderTarget(map, renderer, mapLayer))
                        .ToArray()))
                    .ToDictionary();
                _mapNodes.Add(id, new(configNode, map, new(renderTargets)));
            }
            _mapNode = _mapNodes[id];
            _loaded = true;
        }
        public void Unload()
        {
            Debug.Assert(_loaded);
            _loaded = false;
        }
        public void Draw(Layers layer)
        {
            Debug.Assert(_loaded);
            foreach (ref var renderTarget in _mapNode.RenderTargets[layer].AsSpan())
                Globals.SpriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
        }
        public void Dispose()
        {
            foreach (var mapNode in _mapNodes.Values)
                foreach (var renderTargets in mapNode.RenderTargets.Values)
                    foreach (ref var renderTarget in renderTargets.AsSpan())
                        renderTarget.Dispose();
        }
    }
}
