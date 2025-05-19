using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using MonoGame.Extended;
using System.Diagnostics;

namespace Pow.Utilities
{
    public class BoxesGenerator
    {
        private bool _initialized = false;
        private readonly Dictionary<int, ConfigNode> _configNodes = [];
        private readonly Dictionary<int, Node> _nodes = [];
        private record ConfigNode(string AssetName);
        public record Node(
            Vector2[] Collide,
            Vector2[] Ground,
            ReadOnlyDictionary<Directions, Vector2[]> Walls);
        private static Node Load(string assetName)
        {
            var map = Globals.Game.Content.Load<TiledMap>(assetName);
            var size = new SizeF(
                width: map.WidthInPixels,
                height: map.HeightInPixels);
            var origin = (Vector2)(size / 2);
            var polygonNodes = map.ObjectLayers
                .Where(objectLayer => objectLayer.Name.StartsWith("objects"))
                .SelectMany(objectLayer => objectLayer.Objects.OfType<TiledMapPolygonObject>())
                .Select(polygonObject => new PolygonNode(
                    Position: polygonObject.Position,
                    Vertices: polygonObject.Points,
                    Parameters: new(polygonObject.Properties.ToDictionary(kv => kv.Key, kv => kv.Value.Value))));
            Vector2[] collideBox = null;
            Vector2[] groundBox = null;
            Dictionary<Directions, Vector2[]> wallBoxes = [];
            foreach (var polygonNode in polygonNodes)
            {
                if (polygonNode.Parameters.TryGetValue("BoxType", out var value))
                {
                    var boxType = Enum.Parse<BoxTypes>(value);
                    switch (boxType)
                    {
                        case BoxTypes.Collide:
                            collideBox = polygonNode.Vertices.Select(vertex => vertex + polygonNode.Position - origin).ToArray();
                            break;
                        case BoxTypes.Ground:
                            groundBox = polygonNode.Vertices.Select(vertex => vertex + polygonNode.Position - origin).ToArray();
                            break;
                        case BoxTypes.Wall:
                            {
                                var direction = Enum.Parse<Directions>(polygonNode.Parameters["Direction"]);
                                wallBoxes.Add(direction, polygonNode.Vertices.Select(vertex => vertex + polygonNode.Position - origin).ToArray());
                            }
                            break;
                    }
                }
            }
            var node = new Node(
                Collide: collideBox,
                Ground: groundBox,
                Walls: new(wallBoxes));
            return node;
        }
        public void Configure(int id, string assetName)
        {
            Debug.Assert(!_initialized);
            Debug.Assert(!_configNodes.ContainsKey(id));
            _configNodes.Add(id, new(assetName));
        }
        public void Initialize()
        {
            Debug.Assert(!_initialized);
            _initialized = true;
        }
        public Node GetNode(int id)
        {
            Debug.Assert(_initialized);
            if (!_nodes.ContainsKey(id))
            {
                Debug.Assert(_configNodes.ContainsKey(id));
                _nodes.Add(id, Load(_configNodes[id].AssetName));
            }
            return _nodes[id];
        }
    }
}
