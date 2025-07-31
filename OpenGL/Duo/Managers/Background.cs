using Arch.Core.Extensions;
using Duo.Data;
using Duo.Utilities.Shaders;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
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
using static Pow.Utilities.BoxesGenerator;

namespace Duo.Managers
{
    internal class Background : Environment
    {
        private AnimationManager _parallaxManager;
        private ParallaxFeature _parallaxFeature;
        private Vector2? _velocity;
        private Vector2? _parallax;
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
            var layer = Enum.Parse<Layers>(node.Parameters.GetValueOrDefault("Layer", "FarSky"));
            var animationManager = Entity.Get<AnimationComponent>().Manager;
            animationManager.Play((int)Animations.Background);
            animationManager.Layer = layer;
            animationManager.PositionMode = PositionModes.Screen;
            animationManager.Position = Globals.GameWindowSize / 2;
            animationManager.Show = false;
            _parallaxManager = Pow.Globals.Runner.AnimationGenerator.Acquire();
            _parallaxManager.Play((int)Enum.Parse<Animations>(node.Parameters.GetValueOrDefault("Animation", "Background")));
            _parallaxFeature = animationManager.CreateFeature<ParallaxFeature, ParallaxEffect>();
            _parallaxFeature.Initialize(parallaxParent: _parallaxManager);
            _parallaxFeature.Layer = layer;
            _velocity = GetVector(node.Parameters, "Velocity");
            _parallax = GetVector(node.Parameters, "Parallax");
        }
        public override void Cleanup()
        {
            base.Cleanup();
            _parallaxManager.Return();
        }
        public override void Update()
        {
            base.Update();
            var parallaxPosition = Vector2.Zero;
            if (_velocity.HasValue)
            {
                var totalSeconds = (float)Pow.Globals.GameTime.TotalGameTime.TotalSeconds;
                parallaxPosition += totalSeconds * _velocity.Value;
            }
            if (_parallax.HasValue)
            {
                var cameraPosition = Pow.Globals.Runner.Camera.Position;
                parallaxPosition += cameraPosition * _parallax.Value;
            }
            _parallaxFeature.ParallaxPosition = parallaxPosition;
        }
    }
}
