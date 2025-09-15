using Arch.Core.Extensions;
using Duo.Data;
using Microsoft.Xna.Framework;
using Pow.Components;
using Pow.Utilities;
using Pow.Utilities.Animations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class Image : Environment
    {
        private AnimationManager _animationManager;
        private static Vector2? GetVector(ReadOnlyDictionary<string, string> parameters, string vector)
        {
            if (parameters.ContainsKey(vector))
            {
                var components = parameters
                    .GetValueOrDefault(vector, "0, 0")
                    .Split(",")
                    .Select(x => x.Trim())
                    .Select(x => float.Parse(x))
                    .ToArray();
                Debug.Assert(components.Length == 2);
                return new Vector2(x: components[0], y: components[1]);
            }
            else
            {
                return null;
            }
        }
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            _animationManager = Entity.Get<AnimationComponent>().Manager;
            _animationManager.Play((int)Enum.Parse<Animations>(node.Parameters.GetValueOrDefault("Animation", "Background")));
            _animationManager.Layer = Enum.Parse<Layers>(node.Parameters.GetValueOrDefault("Layer", "FarSky"));
            _animationManager.PositionMode = Enum.Parse<PositionModes>(node.Parameters.GetValueOrDefault("PositionMode", "Screen"));
            var position = GetVector(parameters: node.Parameters, vector: "Position");
            Debug.Assert(position.HasValue);
            _animationManager.Position = position.Value;
        }
        public Vector2 Position
        {
            get => _animationManager.Position;
            set => _animationManager.Position = value;
        }
    }
}
