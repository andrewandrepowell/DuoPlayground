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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateView()
        {
            _view = 
                Matrix.CreateTranslation(new Vector3(-_position, 0f)) * 
                Matrix.CreateTranslation(new Vector3(-_origin, 0f)) * 
                Matrix.CreateRotationZ(_rotation) * 
                Matrix.CreateScale(_zoom, _zoom * _pitch, 1f) * 
                Matrix.CreateTranslation(new Vector3(_origin, 0f));
        }
        public ref Matrix View => ref _view;
        public Vector2 Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _position;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_position == value) return;
                _position = value;
                UpdateView();
            }
        }
        public Vector2 Origin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _origin;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_origin == value) return;
                _origin = value;
                UpdateView();
            }
        }
        public float Rotation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rotation;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_rotation == value) return;
                _rotation = value;
                UpdateView();
            }
        }
        public float Zoom
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _zoom;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (_zoom == value) return;
                _zoom = value;
                UpdateView();
            }
        }
        public float Pitch
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _pitch;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
