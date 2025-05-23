using MonoGame.Extended;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using PowGlobals = Pow.Globals;

namespace Duo.Managers
{
    internal class Camera : Environment
    {
        private string _trackID;
        private DuoObject _duoObjectTracked;
        public DuoObject DuoObjectTracked => _duoObjectTracked;
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            var camera = PowGlobals.Runner.Camera;
            _trackID = node.Parameters["TrackID"];
            _duoObjectTracked = null;
            
            camera.Origin = -(Vector2)(Globals.GameWindowSize / 2);
            camera.Zoom = 2f;
        }
        public override void Cleanup()
        {
            _duoObjectTracked = null;
            base.Cleanup();
        }
        public override void Update()
        {
            var camera = PowGlobals.Runner.Camera;
            if (_duoObjectTracked == null)
            {
                _duoObjectTracked = Globals.DuoRunner.Environments
                    .OfType<DuoObject>()
                    .Where(duoObject => duoObject.ID == _trackID)
                    .First();
            }
            camera.Position = _duoObjectTracked.Position;
            base.Update();
        }
    }
}
