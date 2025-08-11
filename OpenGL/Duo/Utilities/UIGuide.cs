using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Pow.Utilities;

namespace Duo.Utilities
{
    internal static class UIGuide
    {
        public record FrameNode(Vector2? TopLeft, Vector2? TopRight, Vector2? BottomLeft, float? Rotation);
        public record Node(Dictionary<int, FrameNode> FrameNodes);
        public static Node GetNode(MaskGenerator.MaskNode maskNode, Directions direction, Color topLeftColor, Color topRightColor, Color bottomLeftColor)
        {
            var frameNodes = new Dictionary<int, FrameNode>();
            Vector2? GetPosition(Color[] mask, Color match)
            {
                var matches = Enumerable
                    .Range(0, mask.Length)
                    .Where(i => mask[i] == match)
                    .ToArray();
                if (matches.Length == 0)
                    return null;
                return matches
                    .Select(i => new Vector2(x: i % maskNode.RegionSize.Width, y: i / maskNode.RegionSize.Width))
                    .Average();
            }
            foreach ((var maskKey, var mask) in maskNode.Masks)
            {
                if (maskKey.Direction != direction)
                    continue;
                var topLeft = GetPosition(mask: mask, match: topLeftColor);
                var topRight = GetPosition(mask: mask, match: topRightColor);
                var bottomLeft = GetPosition(mask: mask, match: bottomLeftColor);
                float? rotation = null;
                if (topLeft.HasValue && topRight.HasValue)
                {
                    var vector = topRight - topLeft;
                    rotation = (float)System.Math.Atan2(y: vector.Value.Y, vector.Value.X);
                }
                var frameNode = new FrameNode(TopLeft: topLeft, TopRight: topRight, BottomLeft: bottomLeft, Rotation: rotation);
                frameNodes.Add(maskKey.Frame, frameNode);
            }
            return new(frameNodes);
        }
    }
}
