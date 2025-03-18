using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Extended.Tiled;
using Arch.LowLevel;
using System.Diagnostics;
using MonoGame.Extended.Collections;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collisions.Layers;

namespace Pow.Utilities
{
    public class Map
    {
        private readonly Dictionary<int, ConfigNode> _configNodes = [];
        private readonly Dictionary<int, MapNode> _mapNodes = [];
        private MapNode _mapNode;
        private bool _loaded = false;
        private record ConfigNode(string AssetName);
        private record MapNode(ConfigNode Config, TiledMap Map, TiledMapRenderer Renderer, ReadOnlyDictionary<Layers, TiledMapLayer[]> Layers);
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
                var layers = Enum.GetValues<Layers>().Select(x => (x, map.Layers.Where(y => y.Name.StartsWith(x.ToString().ToLower())).ToArray())).ToDictionary();
                _mapNodes.Add(id, new(configNode, map, renderer, new(layers)));
            }
            _mapNode = _mapNodes[id];
            _loaded = true;
        }
        public void Unload()
        {
            Debug.Assert(_loaded);
            _loaded = false;
        }
        public void Update()
        {
            Debug.Assert(_loaded);
            _mapNode.Renderer.Update(Globals.GameTime);
        }
        public void Draw(in Layers layer, in Matrix view, in Matrix projection)
        {
            Debug.Assert(_loaded);
            foreach (ref var mapLayer in _mapNode.Layers[layer].AsSpan())
                _mapNode.Renderer.Draw(layer: mapLayer, view, projection);
        }
    }
}
