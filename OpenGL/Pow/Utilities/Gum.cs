using Gum.DataTypes;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGameGum;
using MonoGameGum.Forms.Controls;
using Pow.Utilities.Animations;
using Pow.Utilities.GO;
using RenderingLibrary;
using RenderingLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Utilities.Gum
{
    public class GumManager : IGOManager
    {
        private readonly GumGenerator _parent;
        private readonly RenderTargetBinding[] _prevRenderTargets;
        private bool _initialized;
        private InteractiveGue _gumRuntime;
        private RenderTarget2D _renderTarget;
        private PositionModes _positionMode;
        private Layers _layers = Layers.Interface;
        private Vector2 _position;
        private SizeF _size;
        private Vector2 _origin;
        private float _visibility;
        private Color _drawColor;
        private void UpdateRenderTarget()
        {
            _renderTarget = new RenderTarget2D(
                graphicsDevice: Globals.SpriteBatch.GraphicsDevice,
                width: (int)_gumRuntime.Width,
                height: (int)_gumRuntime.Height,
                mipMap: false,
                preferredFormat: SurfaceFormat.Color,
                preferredDepthFormat: DepthFormat.None,
                preferredMultiSampleCount: 0,
                usage: RenderTargetUsage.DiscardContents);
        }
        private void UpdateSizeOrigin()
        {
            _size.Width = _renderTarget.Width;
            _size.Height = _renderTarget.Height;
            _origin.X = _size.Width / 2;
            _origin.Y = _size.Height / 2;
        }
        private void UpdateDrawColor()
        {
            _drawColor = Color.White * _visibility;
        }
        public PositionModes PositionMode 
        {
            get => _positionMode;
            set => _positionMode = value; 
        }
        public Layers Layer 
        { 
            get => _layers; 
            set => _layers = value; 
        }
        public Vector2 Position
        {
            get => _position;
            set => _position = value;
        }
        public SizeF Size
        {
            get
            {
                Debug.Assert(_initialized);
                return _size;
            }
        }
        public Vector2 Origin
        {
            get
            {
                Debug.Assert(_initialized);
                return _origin;
            }
        }
        public float Visibility
        {
            get => _visibility;
            set
            {
                _visibility = value;
                UpdateDrawColor();
            }
        }
        public GumProjectSave GumProject => _parent.GumProject;
        public GumManager(GumGenerator parent)
        {
            var graphicsDevice = Globals.SpriteBatch.GraphicsDevice;
            _parent = parent;
            _prevRenderTargets = new RenderTargetBinding[graphicsDevice.RenderTargetCount];
            _positionMode = PositionModes.Screen;
            _visibility = 1;
            _initialized = false;
        }
        public void Initialize(InteractiveGue gumRuntime)
        {
            Debug.Assert(!_initialized);
            _gumRuntime = gumRuntime;
            gumRuntime.AddToRoot(); // needed for ui events.
            UpdateRenderTarget();
            UpdateSizeOrigin();
            UpdateDrawColor();
            _initialized = true;
        }
        public void GumDraw()
        {
            Debug.Assert(_initialized);
            Debug.Assert(_gumRuntime.Width == _renderTarget.Width); // width and height of gumruntime should fit in float mantissa.
            Debug.Assert(_gumRuntime.Height == _renderTarget.Height);
            if (_visibility.EqualsWithTolerance(0)) return;
            var gumBatch = _parent.GumBatch;
            var graphicsDevice = Globals.Game.GraphicsDevice;
            var camera = SystemManagers.Default.Renderer.Camera;
            graphicsDevice.GetRenderTargets(_prevRenderTargets);
            graphicsDevice.SetRenderTargets(_renderTarget);
            graphicsDevice.Clear(Color.Transparent);
            camera.ClientWidth = _renderTarget.Width;
            camera.ClientHeight = _renderTarget.Height;
            gumBatch.Begin();
            gumBatch.Draw(_gumRuntime);
            gumBatch.End();
            graphicsDevice.SetRenderTargets(_prevRenderTargets);
        }
        public void MonoDraw()
        {
            if (_visibility.EqualsWithTolerance(0)) return;
            Globals.SpriteBatch.Draw(
                texture: _renderTarget,
                position: _position,
                sourceRectangle: null,
                color: _drawColor,
                rotation: 0,
                origin: _origin,
                scale: 1,
                effects: SpriteEffects.None,
                layerDepth: 0);
        }
        public void Return()
        {
            Debug.Assert(_initialized);
            _renderTarget.Dispose();
            _gumRuntime.Parent = null;
            _initialized = false;
        }
    }
    public class GumGenerator : IGOGenerator<GumManager>
    {
        private bool _initialized;
        private GumBatch _gumBatch;
        private GumProjectSave _gumProject;
        internal GumBatch GumBatch
        {
            get
            {
                Debug.Assert(_initialized);
                return _gumBatch;
            }
        }
        internal GumProjectSave GumProject
        {
            get
            {
                Debug.Assert(_initialized);
                return _gumProject;
            }
        }
        public GumGenerator(string gumProjectFile = null)
        {
            _initialized = false;
            _gumProject = GumService.Default.Initialize(Globals.Game, gumProjectFile);
            _gumBatch = new GumBatch();
            FrameworkElement.GamePadsForUiControl.Add(GumService.Default.Gamepads[0]);
            FrameworkElement.KeyboardsForUiControl.Add(GumService.Default.Keyboard);
            FrameworkElement.TabKeyCombos.Add(new() { PushedKey = Keys.Down });
            FrameworkElement.TabReverseKeyCombos.Add(new() { PushedKey = Keys.Up });
        }
        public GumManager Acquire()
        {
            Debug.Assert(_initialized);
            return new GumManager(this);
        }
        public void Initialize(int capacity = 64)
        {
            Debug.Assert(!_initialized);
            _initialized = true;
        }
    }
}
