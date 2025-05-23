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
        private bool _debugSet;
        private DuoObject _duoObjectTracked;
        public DuoObject DuoObjectTracked => _duoObjectTracked;
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            var camera = PowGlobals.Runner.Camera;
            var trackID = node.Parameters["TrackID"];
            _duoObjectTracked = Globals.DuoRunner.Environments.OfType<DuoObject>().Where(duoObject => duoObject.ID == trackID).First();
            camera.Origin = -(Vector2)(Globals.GameWindowSize / 2);
            camera.Zoom = 2f;
            _debugSet = false;
        }
        public override void Update()
        {
            var camera = PowGlobals.Runner.Camera;
            //if (!_debugSet)
            //{
                camera.Position = _duoObjectTracked.Position;
                //_debugSet = true;
            //}
            //else
            //{
            //    camera.Position = new(_duoObjectTracked.Position.X, camera.Position.Y);
            //}
            base.Update();
        }
    }
}
