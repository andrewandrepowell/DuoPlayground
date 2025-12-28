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
        private const float _trackBoxPercent = 0.20f;
        private string _trackID;
        private DuoObject _duoObjectTracked;
        private Modes _mode;
        private RectangleF _trackBox = new();
        private bool _updateMode;
        private RectangleF _mapBoundary;
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
        public enum Modes { FullTrack, BoxTrack, CameraWalk }
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            {
                _trackID = node.Parameters["TrackID"];
                _duoObjectTracked = null;
                _mode = Modes.BoxTrack;
                _updateMode = true;
            }
            {
                var camera = Pow.Globals.Runner.Camera;
                camera.Origin = -(Vector2)(Globals.GameWindowSize / 2);
                camera.Zoom = 1f;
            }
            {
                var tiledMap = Pow.Globals.Runner.Map.Node.Map;
                var halfGameWindowSize = (Globals.GameWindowSize / 2);
                var leftMapBoundary = halfGameWindowSize.Width;
                var rightMapBoundary = tiledMap.WidthInPixels - halfGameWindowSize.Width;
                var topMapBoundary = halfGameWindowSize.Height;
                var bottomMapBoundary = tiledMap.HeightInPixels - halfGameWindowSize.Height;
                _mapBoundary = new(x: leftMapBoundary, y: topMapBoundary, width: rightMapBoundary-leftMapBoundary, height: bottomMapBoundary-topMapBoundary);
            }
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
                            // Update track box position based on tracked duo object.
                            var newTrackPosition = _trackBox.Position;
                            if (_trackBox.Left > _duoObjectTracked.Position.X)
                                newTrackPosition.X -= _trackBox.Left - _duoObjectTracked.Position.X;
                            if (_trackBox.Right < _duoObjectTracked.Position.X)
                                newTrackPosition.X -= _trackBox.Right - _duoObjectTracked.Position.X;
                            if (_trackBox.Top > _duoObjectTracked.Position.Y)
                                newTrackPosition.Y -= _trackBox.Top - _duoObjectTracked.Position.Y;
                            if (_trackBox.Bottom < _duoObjectTracked.Position.Y)
                                newTrackPosition.Y -= _trackBox.Bottom - _duoObjectTracked.Position.Y;
                            _trackBox.Position = newTrackPosition;

                            // Update camera position based on track position.
                            camera.Position = _trackBox.Position + (Vector2)_trackBox.Size / 2;
                        }

                        if (!_mapBoundary.Contains(camera.Position))
                        {
                            var newCameraPosition = camera.Position;
                            if (camera.Position.X < _mapBoundary.Left)
                                newCameraPosition.X = _mapBoundary.Left;
                            if (camera.Position.X > _mapBoundary.Right)
                                newCameraPosition.X = _mapBoundary.Right;
                            if (camera.Position.Y < _mapBoundary.Top)
                                newCameraPosition.Y = _mapBoundary.Top;
                            if (camera.Position.Y > _mapBoundary.Bottom)
                                newCameraPosition.Y = _mapBoundary.Bottom;
                            camera.Position = newCameraPosition;
                        }
                    }
                    break;
                case Modes.CameraWalk:
                    {

                    }
                    break;
            }
            base.Update();
        }
    }
}
