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
    public interface IMapParent
    {
        public void Initialize(Map.MapNode mapNode);
    }
    public class Map(IMapParent parent) : IDisposable
    {
        private readonly Dictionary<int, ConfigNode> _configNodes = [];
        private readonly Dictionary<int, MapNode> _mapNodes = [];
        private readonly Queue<int> _idQueue = [];
        private readonly IMapParent _parent = parent;
        private MapNode _mapNode;
        private bool _loaded = false;
        private static RenderTarget2D DrawMapLayerToRenderTarget(TiledMap map, TiledMapRenderer renderer, TiledMapLayer mapLayer)
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
        private void PrivateLoad(int id)
        {
            Debug.Assert(!_loaded);
            if (!_mapNodes.ContainsKey(id))
            {
                var configNode = _configNodes[id];
                var map = Globals.Game.Content.Load<TiledMap>(configNode.AssetName);
                var renderer = new TiledMapRenderer(Globals.SpriteBatch.GraphicsDevice, map);
                var renderTargets = Enum
                    .GetValues<Layers>()
                    .Select(layer => (layer, map.Layers
                        .Where(mapLayer => mapLayer.Name.StartsWith(layer.ToString().ToLower()))
                        .Select(mapLayer => DrawMapLayerToRenderTarget(map, renderer, mapLayer))
                        .ToArray()))
                    .ToDictionary();
                var polygonNodes = map.ObjectLayers
                    .Where(objectLayer => objectLayer.Name.StartsWith("objects"))
                    .SelectMany(objectLayer => objectLayer.Objects.OfType<TiledMapPolygonObject>())
                    .Select(polygonObject => new PolygonNode(
                        Position: polygonObject.Position,
                        Vertices: polygonObject.Points,
                        Parameters: new(polygonObject.Properties.ToDictionary(kv => kv.Key, kv => kv.Value.Value))))
                    .ToArray();
                _mapNodes.Add(id, new(configNode, map, new(renderTargets), polygonNodes));
            }
            _mapNode = _mapNodes[id];
            _parent.Initialize(_mapNode);
            _loaded = true;
        }
        public record ConfigNode(string AssetName);
        public record MapNode(
            ConfigNode Config,
            TiledMap Map,
            ReadOnlyDictionary<Layers, RenderTarget2D[]> RenderTargets,
            PolygonNode[] PolygonNodes);
        public bool Loaded => _loaded;
        public bool Loading => _idQueue.Count > 0;
        public MapNode Node => _mapNode;
        public void Configure(int id, string assetName)
        {
            Debug.Assert(!_loaded);
            Debug.Assert(!_configNodes.ContainsKey(id));
            _configNodes.Add(id, new(assetName));
        }
        public void Load(int id)
        {
            Debug.Assert(_configNodes.ContainsKey(id));
            _idQueue.Enqueue(id);
        }
        public void Unload()
        {
            Debug.Assert(_loaded);
            _loaded = false;
        }
        public void Update()
        {
            while (_idQueue.TryDequeue(out var id)) PrivateLoad(id);
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
