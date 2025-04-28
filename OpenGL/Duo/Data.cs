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
            runner.AnimationGenerator.ConfigureSprite((int)Sprites.Cat, "images/cat_0", new(112, 112));
            runner.AnimationGenerator.ConfigureAnimation((int)Animations.CatWalk, (int)Sprites.Cat, 0, [3, 4, 5, 6, 7, 8], 0.25f, true);
            runner.AnimationGenerator.ConfigureAnimation((int)Animations.CatIdle, (int)Sprites.Cat, 1, [1], 0.25f, false);
            // entities
            runner.AddEntityType((int)EntityTypes.DuoRunner, world => world.Create(new StatusComponent(), new GOCustomComponent<DuoRunner>()));
            runner.AddEntityType((int)EntityTypes.Cat, world => world.Create(new StatusComponent(), new AnimationComponent(), new PhysicsComponent(), new GOCustomComponent<Cat>()));
            runner.AddEntityType((int)EntityTypes.Surface, world => world.Create(new StatusComponent(), new PhysicsComponent(), new GOCustomComponent<Surface>()));
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
