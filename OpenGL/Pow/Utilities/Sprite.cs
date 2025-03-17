using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arch.LowLevel;
using MonoGame.Extended.Collections;

namespace Pow.Utilities.Animation
{
    public class Animation(Manager parent) : IDisposable
    {
        private bool _initialized = true;
        private bool _acquired = false;
        private Manager _parent = parent;
        private UnsafeList<int> _animationIds = [];
        private record struct SpriteNode();
        private record struct AnimationNode(int animationId);
        public void Play(int animationId)
        {
            Debug.Assert(_initialized);
            Debug.Assert(_acquired);
        }
        internal void Acquire()
        {
            Debug.Assert(_initialized);
            Debug.Assert(!_acquired);
            _acquired = true;
        }
        public void Return()
        {
            Debug.Assert(_initialized);
            Debug.Assert(_acquired);
            _parent.Return(this);
            _acquired = false;
        }
        public void Dispose()
        {
            Debug.Assert(_initialized);
            _initialized = false;
        }
    }
    public class Manager : IDisposable
    {
        private Deque<Animation> _animationPool = new();
        private List<SpriteConfigNode> _spriteConfigNodes = [];
        private List<AnimationConfigNode> _animationConfigNodes = [];
        private States _state = States.Configuring;
        internal record SpriteConfigNode(string AssetName, Size RegionSize);
        internal record AnimationConfigNode(int SpriteId, Func<Sprite, int> SpriteConfigure);
        internal List<SpriteConfigNode> SpriteConfigNodes =>  _spriteConfigNodes;
        internal List<AnimationConfigNode> AnimationConfigNodes => _animationConfigNodes;
        public enum States { Configuring, Running, Disposed }
        public int ConfigureSprite(string assetName, Size regionSize)
        {
            Debug.Assert(_state == States.Configuring);
            var spriteId = _spriteConfigNodes.Count;
            _spriteConfigNodes.Add(new(assetName, regionSize));
            return spriteId;
        }
        public int ConfigureAnimation(int spriteId, int[] indices, float period = 0, bool repeat = false)
        {
            Debug.Assert(_state == States.Configuring);
            Debug.Assert(spriteId >= 0 && spriteId < _spriteConfigNodes.Count);
            var animationId = _animationConfigNodes.Count;
            _animationConfigNodes.Add(new(spriteId, (Sprite sprite) => sprite.Configure(indices, period, repeat)));
            return animationId;
        }
        public void Initialize(int capacity = 64)
        {
            Debug.Assert(_state == States.Configuring);
            for (var i = 0; i < capacity; i++)
                _animationPool.AddToBack(new(this));
            _state = States.Running;
        }
        public Animation Acquire()
        {
            Debug.Assert(_state == States.Running);
            var success = _animationPool.RemoveFromFront(out var animation);
            Debug.Assert(success);
            animation.Acquire();
            return animation;
        }
        internal void Return(Animation animation)
        {
            Debug.Assert(_state == States.Running);
            _animationPool.AddToBack(animation);
        }
        public void Dispose()
        {
            Debug.Assert(_state == States.Running);
            _state = States.Disposed;
        }
    }
    public class Sprite : IDisposable
    {
        private readonly Resources<int[]> _intArrayResources = new();
        private readonly Texture2D _texture;
        private readonly Rectangle[] _regions;
        private readonly UnsafeList<Node> _nodes = [];
        private ref Node _node => ref _nodes[_index];
        private float _time;
        private int _frame, _index;
        private bool _initialized = false;
        private bool _running = false;
        private Vector2 _origin;
        private readonly record struct Node(Handle<int[]> Indices, float Period, bool Repeat);
        public Sprite(string assetName, Size regionSize)
        {
            _texture = Globals.ContentManager.Load<Texture2D>(assetName);
            Debug.Assert(_texture.Width % regionSize.Width == 0);
            Debug.Assert(_texture.Height % regionSize.Height == 0);
            var xRegions = _texture.Width / regionSize.Width;
            var yRegions = _texture.Height / regionSize.Height;
            var totalRegions = xRegions * yRegions;
            _regions = new Rectangle[totalRegions];
            for (var y = 0; y < yRegions; y++)
            {
                for (var x = 0; x < xRegions; x++)
                {
                    _regions[x + y * xRegions] = new Rectangle(
                        x * regionSize.Width,
                        y * regionSize.Height,
                        regionSize.Width,
                        regionSize.Height);
                }
            }

            _origin = (regionSize / 2).ToVector2();
        }
        public int Configure(int[] indices, float period = 0, bool repeat = false)
        {
            Debug.Assert(indices.All(x => x >= 0 && x < _regions.Length));
            Debug.Assert(indices.Length > 0);
            Debug.Assert(period >= 0);
            var id = _nodes.Count;
            _nodes.Add(new Node(_intArrayResources.Add(indices), period, repeat));
            return id;
        }
        public void Play(int i)
        {
            _time = _node.Period;
            _frame = 0;
            _index = _intArrayResources.Get(_node.Indices)[_frame];
            _initialized = true;
            _running = true;
        }
        public void Stop()
        {
            _running = false;
        }
        public bool Running => _running;
        public bool Initialized => _initialized;
        public Texture2D Texture => _texture;
        public Rectangle Region
        {
            get
            {
                Debug.Assert(_initialized);
                return _regions[_index];
            }
        }
        public int Index
        {
            get
            {
                Debug.Assert(_initialized);
                return _index;
            }
        }
        public int Frame
        {
            get
            {
                Debug.Assert(_initialized);
                return _frame;
            }
        }
        public Vector2 Origin => _origin;
        public Color Color = Color.White;
        public Vector2 Position;
        public float Scale = 1;
        public float Rotation = 0;
        public SpriteEffects SpriteEffect = SpriteEffects.None;
        public void Update()
        {
            Debug.Assert(_initialized);
            if (_running && _node.Period > 0)
            {
                ref var node = ref _node;
                var nodeIndices = _intArrayResources.Get(node.Indices);
                while (_time <= 0)
                {
                    if (_frame == nodeIndices.Length - 1)
                    {
                        if (_node.Repeat)
                        {
                            _frame = 0;
                            _index = nodeIndices[_frame];
                        }
                        else
                        {
                            _running = false;
                        }
                    }
                    else
                    {
                        _frame++;
                        _index = nodeIndices[_frame];
                    }
                    _time += _node.Period;
                }

                _time -= Globals.GameTime.GetElapsedSeconds();
            }
        }
        public void Draw()
        {
            Debug.Assert(_initialized);
            if (Color.A == 0)
                return;
            Globals.SpriteBatch.Draw(
                texture: _texture,
                position: Position,
                sourceRectangle: Region,
                color: Color,
                rotation: Rotation,
                origin: _origin,
                scale: Scale,
                effects: SpriteEffect,
                layerDepth: 0);
        }
        public void Dispose()
        {
            Debug.Assert(_initialized);
            _intArrayResources.Dispose();
            _nodes.Dispose();
            _initialized = false;
        }
    }
}
