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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class Background : Environment
    {
        private AnimationManager _parallaxManager;
        private ParallaxFeature _parallaxFeature;
        private Vector2? _velocity;
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            var layer = Enum.Parse<Layers>(node.Parameters.GetValueOrDefault("Layer", "FarSky"));
            var animationManager = Entity.Get<AnimationComponent>().Manager;
            animationManager.Play((int)Animations.Background);
            animationManager.Layer = layer;
            animationManager.PositionMode = PositionModes.Screen;
            animationManager.Position = Globals.GameWindowSize / 2;
            animationManager.ShowBase = false;
            _parallaxManager = Pow.Globals.Runner.AnimationGenerator.Acquire();
            _parallaxManager.Play((int)Enum.Parse<Animations>(node.Parameters.GetValueOrDefault("Animation", "Background")));
            _parallaxFeature = animationManager.CreateFeature<ParallaxFeature, ParallaxEffect>();
            _parallaxFeature.Initialize(parallaxParent: _parallaxManager);
            _parallaxFeature.Layer = layer;
            if (node.Parameters.ContainsKey("Velocity"))
            {
                var velocityComponents = node.Parameters
                    .GetValueOrDefault("Velocity", "0, 0")
                    .Split(",")
                    .Select(x => x.Trim())
                    .Select(x => float.Parse(x))
                    .ToArray();
                Debug.Assert(velocityComponents.Length == 2);
                _velocity = new Vector2(x: velocityComponents[0], y: velocityComponents[1]);
            }
            else
            {
                _velocity = null;
            }
        }
        public override void Cleanup()
        {
            base.Cleanup();
            _parallaxManager.Return();
        }
        public override void Update()
        {
            base.Update();
            if (_velocity.HasValue)
            {
                var totalSeconds = (float)Pow.Globals.GameTime.TotalGameTime.TotalSeconds;
                _parallaxFeature.ParallaxPosition = totalSeconds * _velocity.Value;
            }
        }
    }
}
