using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly static Directions[] _directions = Enum.GetValues<Directions>();
        private readonly SizeF _gameWindowSize;
        private readonly SizeF _screenWindowSize;
        private Vector2 _offset;
        private float _scalar;
        private interface IUpdateLetterBoxNode
        {
            public void Update(Rectangle destinationRectangle);
        }
        public class LetterBoxNode : IUpdateLetterBoxNode
        {
            private Rectangle _destinationRectangle;
            void IUpdateLetterBoxNode.Update(Rectangle destinationRectangle) => 
                _destinationRectangle = destinationRectangle;
            public Rectangle DestinationRectangle => _destinationRectangle;
        }
        private ReadOnlyDictionary<Directions, LetterBoxNode> _letterBoxNodes;
        public Vector2 Offset => _offset;
        public float Scalar => _scalar;
        public ReadOnlyDictionary<Directions, LetterBoxNode> LetterBoxNodes => _letterBoxNodes;
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
            {
                var letterBoxNodes = new Dictionary<Directions, LetterBoxNode>();
                foreach (var direction in _directions)
                    letterBoxNodes.Add(direction, new LetterBoxNode());
                _letterBoxNodes = new(letterBoxNodes);
            }
        }
        public void Update()
        {
            // Compute the scalar and offset.
            {
                var graphicsAdapter = GraphicsAdapter.DefaultAdapter;
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

            // Compute the letter box destination rectangles.
            {
                var scaledGameWindowSize = new SizeF(
                    width: _gameWindowSize.Width * _scalar,
                    height: _gameWindowSize.Height * _scalar);
                foreach (var direction in _directions)
                {
                    var letterBoxNode = (IUpdateLetterBoxNode)_letterBoxNodes[direction];
                    switch (direction)
                    {
                        case Directions.Up:
                            letterBoxNode.Update(new(
                                x: 0,
                                y: 0,
                                width: (int)Math.Ceiling(_screenWindowSize.Width),
                                height: (int)Math.Ceiling(_offset.Y)));
                            break;
                        case Directions.Down:
                            letterBoxNode.Update(new(
                                x: 0,
                                y: (int)Math.Ceiling(_offset.Y + scaledGameWindowSize.Height),
                                width: (int)Math.Ceiling(_screenWindowSize.Width),
                                height: (int)Math.Ceiling(_offset.Y)));
                            break;
                        case Directions.Left:
                            letterBoxNode.Update(new(
                                x: 0,
                                y: 0,
                                width: (int)Math.Ceiling(_offset.X),
                                height: (int)Math.Ceiling(_screenWindowSize.Height)));
                            break;
                        case Directions.Right:
                            letterBoxNode.Update(new(
                                x: (int)Math.Ceiling(_offset.X + scaledGameWindowSize.Width),
                                y: 0,
                                width: (int)Math.Ceiling(_offset.X),
                                height: (int)Math.Ceiling(_screenWindowSize.Height)));
                            break; 
                    }
                }
            }
        }
    }
}
