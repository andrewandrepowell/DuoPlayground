using Gum.Wireframe;
using Microsoft.Xna.Framework.Graphics;
using Pow.Utilities.Animations;
using Pow.Utilities.GO;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RenderingLibrary.Graphics;
using MonoGameGum;
using MonoGame.Extended;
using RenderingLibrary;
using Gum.DataTypes;

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
        public enum PositionModes { Screen, Map }
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
        public GumProjectSave GumProject => _parent.GumProject;
        public GumManager(GumGenerator parent)
        {
            var graphicsDevice = Globals.SpriteBatch.GraphicsDevice;
            _parent = parent;
            _prevRenderTargets = new RenderTargetBinding[graphicsDevice.RenderTargetCount];
            _positionMode = PositionModes.Screen;
            _initialized = false;
        }
        public void Initialize(InteractiveGue gumRuntime)
        {
            Debug.Assert(!_initialized);
            _gumRuntime = gumRuntime;
            UpdateRenderTarget();
            UpdateSizeOrigin();
            _initialized = true;
        }
        public void GumDraw()
        {
            Debug.Assert(_initialized);
            Debug.Assert(_gumRuntime.Width == _renderTarget.Width); // width and height of gumruntime should fit in float mantissa.
            Debug.Assert(_gumRuntime.Height == _renderTarget.Height);
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
            Globals.SpriteBatch.Draw(
                texture: _renderTarget,
                position: _position,
                sourceRectangle: null,
                color: Color.White,
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
