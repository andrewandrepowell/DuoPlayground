using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Utilities
{
    public class MaskGenerator
    {
        private bool _initialized = false;
        private readonly static Directions[] _directions = [Directions.Left, Directions.Right];
        private readonly Dictionary<int, ConfigNode> _configNodes = [];
        private readonly Dictionary<int, MaskNode> _maskNodes = [];
        public record ConfigNode(string AssetName, Size RegionSize, ReadOnlyDictionary<Directions, SpriteEffects> DirectionSpriteEffects);
        public readonly record struct MaskKey(Directions Direction, int Frame);
        public record MaskNode(ReadOnlyDictionary<MaskKey, Color[]> Masks, Size RegionSize);
        public void Configure(int id, string assetName, Size regionSize, ReadOnlyDictionary<Directions, SpriteEffects> directionSpriteEffects)
        {
            Debug.Assert(!_initialized);
            Debug.Assert(!_configNodes.ContainsKey(id));
            _configNodes.Add(id, new(assetName, regionSize, directionSpriteEffects));
        }
        public void Load(int id)
        {
            Debug.Assert(_initialized);
            Debug.Assert(_configNodes.ContainsKey(id));
            Debug.Assert(!_maskNodes.ContainsKey(id));
            var configNode = _configNodes[id];
            var spriteTexture = Globals.Game.Content.Load<Texture2D>(configNode.AssetName);
            Debug.Assert(spriteTexture.Width % configNode.RegionSize.Width == 0);
            Debug.Assert(spriteTexture.Height % configNode.RegionSize.Height == 0);
            var spriteData = new Color[spriteTexture.Width * spriteTexture.Height];
            spriteTexture.GetData(spriteData);
            var xRegions = spriteTexture.Width / configNode.RegionSize.Width;
            var yRegions = spriteTexture.Height / configNode.RegionSize.Height;
            var frame = 0;
            var masks = new Dictionary<MaskKey, Color[]>();
            for (var yRegion = 0; yRegion < yRegions; yRegion++)
            {
                var yPixBase = yRegion * configNode.RegionSize.Height;
                for (var xRegion = 0; xRegion < xRegions; xRegion++)
                {
                    var xPixBase = xRegion * configNode.RegionSize.Width;
                    foreach (ref var direction in _directions.AsSpan())
                    {
                        var maskData = new Color[configNode.RegionSize.Width * configNode.RegionSize.Height];
                        for (var y = 0; y < configNode.RegionSize.Height; y++)
                        {
                            for (var x = 0; x < configNode.RegionSize.Width; x++)
                            {
                                var xPix = xPixBase + ((configNode.DirectionSpriteEffects[direction] == SpriteEffects.FlipHorizontally) ? configNode.RegionSize.Width - 1 - x : x);
                                var yPix = yPixBase + ((configNode.DirectionSpriteEffects[direction] == SpriteEffects.FlipVertically) ? configNode.RegionSize.Height - 1 - y : y);
                                var spriteIndex = xPix + yPix * spriteTexture.Width;
                                var maskIndex = x + y * configNode.RegionSize.Width;
                                maskData[maskIndex] = spriteData[spriteIndex];
                            }
                        }
                        var maskKey = new MaskKey(Direction: direction, Frame: frame);
                        masks.Add(maskKey, maskData);
                    }
                    frame++;
                }
            }
            var maskNode = new MaskNode(Masks: new(masks), RegionSize: configNode.RegionSize);
            _maskNodes.Add(id, maskNode);
            Globals.Game.Content.UnloadAsset(configNode.AssetName);
        }
        public void Initialize(bool load = false)
        {
            Debug.Assert(!_initialized);
            _initialized = true;
            if (load)
            {
                foreach (var id in _configNodes.Keys)
                    Load(id);
            }
        }
        public MaskNode GetNode(int id)
        {
            Debug.Assert(_initialized);
            Debug.Assert(_configNodes.ContainsKey(id));
            if (!_maskNodes.ContainsKey(id)) Load(id);
            return _maskNodes[id];
        }
    }
}
