using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;


namespace Pow.Utilities
{
    internal class GameWindow
    {
        private readonly SizeF _gameWindowSize;
        private readonly SizeF _screenWindowSize;
        private Vector2 _offset;
        private float _scalar;
        public Vector2 Offset => _offset;
        public float Scalar => _scalar;
        public GameWindow(SizeF gameWindowSize)
        {
            _gameWindowSize = gameWindowSize;
            var graphicsAdapter = GraphicsAdapter.DefaultAdapter;
            _screenWindowSize = new SizeF(
                width: graphicsAdapter.CurrentDisplayMode.Width,
                height: graphicsAdapter.CurrentDisplayMode.Height);
            Globals.Game.IsMouseVisible = false;
            var graphicsDeviceManager = Globals.GraphicsDeviceManager;
            graphicsDeviceManager.PreferredBackBufferWidth = (int)_screenWindowSize.Width;
            graphicsDeviceManager.PreferredBackBufferHeight = (int)_screenWindowSize.Height;
            graphicsDeviceManager.IsFullScreen = true;
            graphicsDeviceManager.HardwareModeSwitch = false;
            graphicsDeviceManager.ApplyChanges();
        }
        public void Update()
        {
            var widthScalar = _screenWindowSize.Width / _gameWindowSize.Width;
            var heightScaledByWidthScalar = _gameWindowSize.Height * widthScalar;
            _offset = Vector2.Zero;
            if (heightScaledByWidthScalar < _screenWindowSize.Height || heightScaledByWidthScalar.EqualsWithTolerance(_screenWindowSize.Height))
            {
                _scalar = widthScalar;
                _offset.Y = (_screenWindowSize.Height - heightScaledByWidthScalar) / 2;
            }
            else
            {
                _scalar = _screenWindowSize.Height / _gameWindowSize.Height;
                var widthScaledByScalar = _gameWindowSize.Width * _scalar;
                _offset.X = (_screenWindowSize.Width - widthScaledByScalar) / 2;
            }
        }
    }
}
