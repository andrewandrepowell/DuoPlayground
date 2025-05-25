using MonoGame.Extended;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Duo.Managers
{
    internal class Camera : Environment
    {
        private const float _trackBoxPercent = 0.40f;
        private string _trackID;
        private DuoObject _duoObjectTracked;
        private Modes _mode;
        private RectangleF _trackBox = new();
        private bool _updateMode;
        public DuoObject DuoObjectTracked => _duoObjectTracked;
        public Modes Mode
        {
            get => _mode;
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    _updateMode = true;
                }
            }
        }
        public enum Modes { FullTrack, BoxTrack }
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            var camera = Pow.Globals.Runner.Camera;
            _trackID = node.Parameters["TrackID"];
            _duoObjectTracked = null;
            _mode = Modes.BoxTrack;
            _updateMode = true;
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
            var camera = Pow.Globals.Runner.Camera;
            if (_duoObjectTracked == null)
            {
                _duoObjectTracked = Globals.DuoRunner.Environments
                    .OfType<DuoObject>()
                    .Where(duoObject => duoObject.ID == _trackID)
                    .First();
            }
            if (_updateMode)
            {
                switch (_mode)
                {
                    case Modes.BoxTrack:
                        {
                            var gameWindowSize = Globals.GameWindowSize;
                            _trackBox.Width = gameWindowSize.Width / camera.Zoom * _trackBoxPercent;
                            _trackBox.Height = gameWindowSize.Height / camera.Zoom * _trackBoxPercent;
                            _trackBox.Position = _duoObjectTracked.Position - (Vector2)_trackBox.Size / 2;
                            camera.Position = _duoObjectTracked.Position;
                        }
                        break;
                }
                _updateMode = false;
            }
            switch (_mode)
            {
                case Modes.FullTrack:
                    {
                        camera.Position = _duoObjectTracked.Position;
                    }
                    break;
                case Modes.BoxTrack:
                    {
                        if (!_trackBox.Contains(_duoObjectTracked.Position))
                        {
                            var newPosition = _trackBox.Position;
                            if (_trackBox.Left > _duoObjectTracked.Position.X)
                                newPosition.X -= _trackBox.Left - _duoObjectTracked.Position.X;
                            if (_trackBox.Right < _duoObjectTracked.Position.X)
                                newPosition.X -= _trackBox.Right - _duoObjectTracked.Position.X;
                            if (_trackBox.Top > _duoObjectTracked.Position.Y)
                                newPosition.Y -= _trackBox.Top - _duoObjectTracked.Position.Y;
                            if (_trackBox.Bottom < _duoObjectTracked.Position.Y)
                                newPosition.Y -= _trackBox.Bottom - _duoObjectTracked.Position.Y;
                            _trackBox.Position = newPosition;
                            camera.Position = _trackBox.Position + (Vector2)_trackBox.Size / 2;
                        }
                    }
                    break;
            }
            base.Update();
        }
    }
}
