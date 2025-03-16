﻿using System;
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

namespace Pow.Utilities
{
    public class Map
    {
        private readonly List<ConfigNode> _configNodes = [];
        private readonly Dictionary<int, MapNode> _mapNodes = [];
        private MapNode _mapNode;
        private bool _loaded = false;
        private record ConfigNode(string AssetName);
        private record MapNode(ConfigNode Config, TiledMap Map, TiledMapRenderer Renderer, ReadOnlyDictionary<Layers, TiledMapLayer[]> Layers);
        public bool Loaded => _loaded;
        public int Configure(string assetName)
        {
            Debug.Assert(!_loaded);
            Debug.Assert(Globals.State >= Globals.States.WaitingForInitPow);
            var id = _configNodes.Count;
            _configNodes.Add(new(assetName));
            return id;
        }
        public void Load(int id)
        {
            Debug.Assert(!_loaded);
            Debug.Assert(Globals.State >= Globals.States.WaitingForInitPow);
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

            _loaded = false;
        }
        public void Update()
        {
            _mapNode.Renderer.Update(Globals.GameTime);
        }
        public void Draw(in Layers layer, in Matrix view)
        {
            Debug.Assert(_loaded);
            var gd = Globals.SpriteBatch.GraphicsDevice;
            var p = Matrix.CreateOrthographicOffCenter(0, gd.Viewport.Width, gd.Viewport.Height, 0, 0, -1); // DEBUG NEEDS FIXING
            foreach (ref var mapLayer in _mapNode.Layers[layer].AsSpan())
                _mapNode.Renderer.Draw(layer: mapLayer, view, p);
        }
    }
}
