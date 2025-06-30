using Duo.Data;
using Pow.Utilities;
using Pow.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.Core.Extensions;
using Pow.Utilities.Animations;
using Duo.Utilities.Shaders;

namespace Duo.Managers
{
    internal class Background : Environment
    {
        private AnimationManager _parallaxManager;
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            var animationManager = Entity.Get<AnimationComponent>().Manager;
            animationManager.Play((int)Animations.Background);
            animationManager.Layer = Layers.FarSky;
            animationManager.PositionMode = PositionModes.Screen;
            animationManager.Position = Globals.GameWindowSize / 2;
            _parallaxManager = Pow.Globals.Runner.AnimationGenerator.Acquire();
            _parallaxManager.Play((int)Enum.Parse<Animations>(node.Parameters.GetValueOrDefault("Animation", "Background")));
            var parallaxFeature = animationManager.CreateFeature<ParallaxFeature, ParallaxEffect>();
            parallaxFeature.Initialize(parallaxParent: _parallaxManager);
            parallaxFeature.Layer = Layers.FarSky;
        }
        public override void Cleanup()
        {
            base.Cleanup();
            _parallaxManager.Return();
        }
    }
}
