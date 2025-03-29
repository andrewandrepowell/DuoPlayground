using Duo.Managers;
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
    internal enum Animations { CatWalk, CatIdle }
    public enum EntityTypes { Initialize, Wall, Cat }
    public class DataInitializer : IRunnerParent, IDuoRunnerParent
    {
        public DataInitializer()
        {
            DuoRunner.Initialize(this);
        }
        public void Initialize(Runner runner)
        {
            throw new NotImplementedException();
        }
        public void Initialize(DuoRunner duoRunner)
        {
            duoRunner.Add<Cat>(EntityTypes.Cat);
            duoRunner.Add<Wall>(EntityTypes.Wall);
        }
        public void Initialize(Map.MapNode node)
        {
            var duoRunner = Globals.DuoRunner;
            foreach (ref var polygonNode in node.PolygonNodes.AsSpan())
                duoRunner.Add(polygonNode);
            
        }
    }
}
