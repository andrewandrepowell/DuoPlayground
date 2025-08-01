using Duo.Data;
using Duo.Utilities.Shaders;
using Pow.Utilities;
using Pow.Utilities.Animations;
using Pow.Utilities.Shaders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal class Collectible : Interactable
    {
        private readonly static ReadOnlyDictionary<Modes, Animations> _modeAnimations = new(new Dictionary<Modes, Animations>() 
        {
            { Modes.PineCone, Animations.PineCone },
        });
        private static readonly ReadOnlyDictionary<Actions, int> _actionAnimationGroupMap = new(new Dictionary<Actions, int>() 
        {
            { Actions.Waiting, (int)AnimationGroups.Idle }
        });
        private Modes _mode;
        private ShineFeature _shineFeature;
        private FloatFeature _floatFeature;
        private RotationFeature _rotationFeature;
        private DriftAwayFeature _driftAwayFeature;
        private VanishFeature _vanishFeature;
        private enum AnimationGroups { Idle }
        public enum Modes { PineCone }
        protected override IReadOnlyDictionary<Actions, int> ActionAnimationGroupMap => _actionAnimationGroupMap;
        protected override Boxes Boxes => Boxes.Collectible;
        protected override Layers Layer => Layers.Foreground;
        protected override bool Solid => false;
        public Modes Mode => _mode;
        protected override void Initialize(AnimationGroupManager manager)
        {
            base.Initialize(manager);
            manager.Configure(
                groupId: (int)AnimationGroups.Idle,
                group: new PlaySingleGroup((int)_modeAnimations[_mode]));

        }
        public override void Initialize(PolygonNode node)
        {
            _mode = Enum.Parse<Modes>(node.Parameters.GetValueOrDefault("Mode", "PineCone")); // Mode needs to be configured first.
            base.Initialize(node);
            {
                var animationManager = AnimationManager;
                _shineFeature = animationManager.CreateFeature<ShineFeature, ShineEffect>();
                _shineFeature.Layer = Layer;
                _shineFeature.Start();
                _floatFeature = animationManager.CreateFeature<FloatFeature, NullEffect>();
                _rotationFeature = animationManager.CreateFeature<RotationFeature, NullEffect>();
                _driftAwayFeature = animationManager.CreateFeature<DriftAwayFeature, NullEffect>();
                _vanishFeature = animationManager.CreateFeature<VanishFeature, NullEffect>();
            }
        }
        public override void Interact()
        {
            base.Interact();
            _rotationFeature.Start();
            _driftAwayFeature.Start();
            _vanishFeature.Start();
        }
    }
}
