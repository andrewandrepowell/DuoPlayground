using Duo.Managers;
using Pow.Components;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace Duo.Data
{
    internal enum Maps { LevelDebug0 }
    internal enum Sprites { Cat }
    internal enum Animations { CatWalk, CatIdle }
    public enum EntityTypes { DuoRunner, Surface, Cat }
    public class DataInitializer : IRunnerParent, IDuoRunnerParent
    {
        public DataInitializer()
        {
            DuoRunner.Initialize(this);
        }
        public void Initialize(Runner runner)
        {
            // maps
            runner.Map.Configure((int)Maps.LevelDebug0, "tiled/test_map_0");
            // sprites / animations.
            runner.AnimationGenerator.ConfigureSprite(
                spriteId: (int)Sprites.Cat, 
                assetName: "images/cat_0", 
                regionSize: new(112, 112));
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
            runner.AddGOCustomManager<Cat>();
            runner.AddGOCustomManager<Surface>();
        }
        public void Initialize(DuoRunner duoRunner)
        {
            duoRunner.Add<Cat>(EntityTypes.Cat);
            duoRunner.Add<Surface>(EntityTypes.Surface);
        }
        public void Initialize(Map.MapNode node)
        {
            var duoRunner = Globals.DuoRunner;
            foreach (ref var polygonNode in node.PolygonNodes.AsSpan())
                duoRunner.Add(polygonNode);
            
        }
    }
}
