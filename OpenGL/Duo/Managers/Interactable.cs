using Arch.Core.Extensions;
using Duo.Data;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
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
    internal abstract class Interactable : DuoObject
    {
        private AnimationGroupManager _animationGroupManager;
        private Actions _action;
        private Fixture _fixture;
        protected abstract IReadOnlyDictionary<Actions, int> ActionAnimationGroupMap { get; }
        protected abstract Boxes Boxes { get; }
        private void UpdateAction(Actions action)
        {
            if (ActionAnimationGroupMap.TryGetValue(action, out var groupId))
            {
                var animationManager = AnimationManager;
                _animationGroupManager.Play(groupId);
            }
            _action = action;
        }
        protected virtual bool FinishedInteracting => !AnimationManager.Running;
        public enum Actions { Waiting, Interacting, Interacted }
        public Actions Action => _action;
        protected virtual void Initialize(AnimationGroupManager manager)
        {
        }
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            {
                var body = Entity.Get<PhysicsComponent>().Manager.Body;
                var boxesNode = Globals.DuoRunner.BoxesGenerator.GetNode((int)Boxes);
                body.BodyType = BodyType.Static;
                body.Tag = this;
                var shape = new PolygonShape(
                        vertices: new(boxesNode.Collide.Select(
                            pixelPosition => pixelPosition / Globals.PixelsPerMeter)),
                        density: 1f);
                var fixture = new Fixture(shape);
                fixture.Friction = 3f;
                fixture.IsSensor = false;
                fixture.CollisionCategories = Category.Cat1;
                body.Add(fixture);
                _fixture = fixture; // Will use to disable and enable collisions.
                body.Position = (node.Vertices.Average() + node.Position) / Globals.PixelsPerMeter;
            }
            {
                _animationGroupManager = new(AnimationManager);
                Initialize(manager: _animationGroupManager);
                _animationGroupManager.Initialize();
            }
            UpdateAction(Actions.Waiting);
        }
        public virtual void Interact()
        {
            Debug.Assert(Action == Actions.Waiting);
            _fixture.CollisionCategories = Category.None;
            UpdateAction(Actions.Interacting);
        }
        public virtual void FinishInteracting()
        {
            Debug.Assert(Action == Actions.Interacting);
            _fixture.CollisionCategories = Category.None;
            UpdateAction(Actions.Interacted);
        }
        public override void Update()
        {
            base.Update();
            if (Pow.Globals.GamePaused) 
                return;
            if (Action == Actions.Interacting && FinishedInteracting)
                FinishInteracting();
            _animationGroupManager.Update();
        }
    }
}
