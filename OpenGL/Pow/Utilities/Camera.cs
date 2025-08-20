using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Utilities
{
    public class Camera
    {
        private Vector2 _position;
        private Vector2 _origin;
        private float _rotation;
        private float _zoom;
        private float _pitch;
        private Matrix _view;
        private Matrix _projection;
        private Matrix _viewProjection;
        private Viewport _viewport;
        private void UpdateView()
        {
            _view = 
                Matrix.CreateTranslation(new Vector3(-_position, 0f)) * 
                Matrix.CreateRotationZ(_rotation) * 
                Matrix.CreateScale(_zoom, _zoom * _pitch, 1f) *
                Matrix.CreateTranslation(new Vector3(-_origin, 0f));
        }
        private void UpdateViewProjection()
        {
            _viewProjection = _view * _projection;
        }
        private void UpdateProjection()
        {
            var viewport = Globals.Game.GraphicsDevice.Viewport;
            if (viewport.Width != _viewport.Width || viewport.Height != _viewport.Height)
            {
                _viewport = viewport;
                _projection = Matrix.CreateOrthographicOffCenter(0f, _viewport.Width, _viewport.Height, 0f, 0f, -1f);
                UpdateViewProjection();
            }
        }
        public ref Matrix View => ref _view;
        public ref Matrix Projection
        {
            get
            {
                UpdateProjection();
                return ref _projection;
            }
        }
        public ref Matrix ViewProjection
        {
            get
            {
                UpdateProjection();
                return ref _viewProjection;
            }
        }
        public Vector2 Position
        {
            get => _position;
            set
            {
                if (_position == value) return;
                _position = value;
                UpdateView();
                UpdateViewProjection();
            }
        }
        public Vector2 Origin
        {
            get => _origin;
            set
            {
                if (_origin == value) return;
                _origin = value;
                UpdateView();
                UpdateViewProjection();
            }
        }
        public float Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation == value) return;
                _rotation = value;
                UpdateView();
                UpdateViewProjection();
            }
        }
        public float Zoom
        {
            get => _zoom;
            set
            {
                if (_zoom == value) return;
                _zoom = value;
                UpdateView();
                UpdateViewProjection();
            }
        }
        public float Pitch
        {
            get => _pitch;
            set
            {
                if (_pitch == value) return;
                _pitch = value;
                UpdateView();
                UpdateViewProjection();
            }
        }
        public Camera()
        {
            _position = Vector2.Zero;
            _origin = Vector2.Zero;
            _rotation = 0;
            _zoom = 1;
            _pitch = 1;
            UpdateView();
            UpdateProjection();
        }
    }
}
