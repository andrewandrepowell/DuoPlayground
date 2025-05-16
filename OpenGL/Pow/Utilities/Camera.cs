using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using MonoGame.Extended;
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
        private void UpdateView()
        {
            _view = 
                Matrix.CreateTranslation(new Vector3(-_position, 0f)) * 
                Matrix.CreateRotationZ(_rotation) * 
                Matrix.CreateScale(_zoom, _zoom * _pitch, 1f) *
                Matrix.CreateTranslation(new Vector3(-_origin, 0f));

            /*
             *             _view = 
                Matrix.CreateTranslation(new Vector3(-_position, 0f)) * 
                Matrix.CreateTranslation(new Vector3(-_origin, 0f)) * 
                Matrix.CreateRotationZ(_rotation) * 
                Matrix.CreateScale(_zoom, _zoom * _pitch, 1f) * 
                Matrix.CreateTranslation(new Vector3(_origin / _zoom, 0f));
            */
        }
        public ref Matrix View => ref _view;
        public Vector2 Position
        {
            get => _position;
            set
            {
                if (_position == value) return;
                _position = value;
                UpdateView();
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
        }
    }
}
