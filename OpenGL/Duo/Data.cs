using Duo.Managers;
using Microsoft.Xna.Framework.Graphics;
using Pow.Components;
using Pow.Utilities;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace Duo.Data
{
    internal enum Maps { LevelDebug0, LevelDebug1 }
    internal enum Sprites { Cat }
    internal enum Animations { CatWalk, CatIdle }
    internal enum Boxes { Cat }
    public enum EntityTypes { DuoRunner, Camera, Surface, Cat }
    public class Data : IRunnerParent, IDuoRunnerParent
    {
        public Data() => DuoRunner.Initialize(this);
        public SizeF GameWindowSize => Globals.GameWindowSize;
        public float PixelsPerMeter => Globals.PixelsPerMeter;
        public void Initialize(Runner runner)
        {
            // maps
            runner.Map.Configure((int)Maps.LevelDebug0, "tiled/test_map_0");
            runner.Map.Configure((int)Maps.LevelDebug1, "tiled/test_map_1");
            // sprites / animations.
            runner.AnimationGenerator.ConfigureSprite(
                spriteId: (int)Sprites.Cat, 
                assetName: "images/cat_0", 
                regionSize: new(112, 112),
                directionSpriteEffects: new(new Dictionary<Directions, SpriteEffects>() 
                {
                    {Directions.Left, SpriteEffects.None},
                    {Directions.Right, SpriteEffects.FlipHorizontally},
                }));
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.CatWalk,
                spriteId: (int)Sprites.Cat, 
                spriteAnimationId: 0, 
                indices: [3, 4, 5, 6, 7, 8], 
                period: 0.25f, 
                repeat: true);
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.CatIdle,
                spriteId: (int)Sprites.Cat,
                spriteAnimationId: 1,
                indices: [1],
                period: 0.25f,
                repeat: false);
            // entities
            runner.AddEntityType((int)EntityTypes.DuoRunner, world => world.Create(
                new StatusComponent(), 
                new GOCustomComponent<DuoRunner>()));
            runner.AddEntityType((int)EntityTypes.Camera, world => world.Create(
                new StatusComponent(),
                new GOCustomComponent<Managers.Camera>()));
            runner.AddEntityType((int)EntityTypes.Cat, world => world.Create(
                new StatusComponent(), 
                new AnimationComponent(),
                new PhysicsComponent(),
                new ControlComponent(),
                new GOCustomComponent<Cat>()));
            runner.AddEntityType((int)EntityTypes.Surface, world => world.Create(
                new StatusComponent(), 
                new PhysicsComponent(), 
                new GOCustomComponent<Surface>()));
            // custom GO managers.
            runner.AddGOCustomManager<DuoRunner>();
            runner.AddGOCustomManager<Managers.Camera>();
            runner.AddGOCustomManager<Cat>();
            runner.AddGOCustomManager<Surface>();
        }
        public void Initialize(DuoRunner duoRunner)
        {
            // Environment
            duoRunner.AddEnvironment<Managers.Camera>(EntityTypes.Camera);
            duoRunner.AddEnvironment<Cat>(EntityTypes.Cat);
            duoRunner.AddEnvironment<Surface>(EntityTypes.Surface);
            // Boxes
            duoRunner.BoxesGenerator.Configure(
                id: (int)Boxes.Cat, 
                assetName: "tiled/cat_boxes_0");
        }
        public void Initialize(Map.MapNode node)
        {
            var duoRunner = Globals.DuoRunner;
            foreach (ref var polygonNode in node.PolygonNodes.AsSpan())
                duoRunner.AddEnvironment(polygonNode);
        }
    }
}
