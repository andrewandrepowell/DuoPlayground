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
        private enum AnimationGroups { Idle }
        public enum Modes { PineCone }
        protected override IReadOnlyDictionary<Actions, int> ActionAnimationGroupMap => _actionAnimationGroupMap;
        protected override Boxes Boxes => Boxes.Collectible;
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
                var shineFeature = animationManager.CreateFeature<ShineFeature, ShineEffect>();
                shineFeature.Start();
                _ = animationManager.CreateFeature<FloatFeature, NullEffect>();
            }
        }
    }
}
