using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace Pow.Utilities.Animations
{
    public class AnimationManager(AnimationGenerator parent)
    {
        private bool _acquired = false;
        private AnimationGenerator _parent = parent;
        private readonly Dictionary<int, SpriteNode> _spriteNodes = [];
        private readonly Dictionary<int, AnimationNode> _animationNodes = [];
        private AnimationNode _animationNode;
        private int? _animationId = null;
        private Vector2 _position;
        private record SpriteNode(Sprite Sprite);
        private record AnimationNode(SpriteNode SpriteNode, int SpriteAnimationId);
        private void UpdateSpritePosition()
        {
            Debug.Assert(_animationId != null);
            var sprite = _animationNode.SpriteNode.Sprite;
            sprite.Position.X = (float)Math.Floor(_position.X);
            sprite.Position.Y = (float)Math.Floor(_position.Y);
        }
        public int AnimationId
        {
            get
            {
                Debug.Assert(_animationId != null);
                return _animationId.Value;
            }
        }
        public bool Running
        {
            get
            {
                Debug.Assert(_animationId != null);
                return _animationNode.SpriteNode.Sprite.Running;
            }
        }
        public Vector2 Position
        {
            get => _position;
            set
            {
                Debug.Assert(_animationId != null);
                if (_position == value) return;
                _position = value;
                UpdateSpritePosition();
            }
        }
        public void LoadSprite(int spriteId)
        {
            Debug.Assert(!_spriteNodes.ContainsKey(spriteId));
            Debug.Assert(_parent.SpriteConfigNodes.ContainsKey(spriteId));
            var spriteConfigNode = _parent.SpriteConfigNodes[spriteId];
            var sprite = new Sprite(spriteConfigNode.AssetName, spriteConfigNode.RegionSize);
            var spriteNode = new SpriteNode(sprite);
            _spriteNodes.Add(spriteId, spriteNode);
        }
        public void LoadAnimation(int animationId)
        {
            Debug.Assert(!_animationNodes.ContainsKey(animationId));
            Debug.Assert(_parent.AnimationConfigNodes.ContainsKey(animationId));
            var animationConfigNode = _parent.AnimationConfigNodes[animationId];
            var spriteId = animationConfigNode.SpriteId;
            if (!_spriteNodes.ContainsKey(spriteId))
                LoadSprite(spriteId);
            var spriteNode = _spriteNodes[spriteId];
            var sprite = spriteNode.Sprite;
            sprite.Configure(animationConfigNode.SpriteAnimationId, animationConfigNode.Indices, animationConfigNode.Period, animationConfigNode.Repeat);
            var animationNode = new AnimationNode(spriteNode, animationConfigNode.SpriteAnimationId);
            _animationNodes.Add(animationId, animationNode);
        }
        public void Play(int animationId)
        {
            Debug.Assert(_acquired);
            if (animationId != _animationId)
            {
                if (!_animationNodes.ContainsKey(animationId))
                    LoadAnimation(animationId);
                _animationId = animationId;
                _animationNode = _animationNodes[animationId];
            }
            _animationNode.SpriteNode.Sprite.Play(_animationNode.SpriteAnimationId);
            UpdateSpritePosition();
        }
        public void Stop()
        {
            Debug.Assert(_acquired);
            Debug.Assert(_animationId != null);
            _animationNode.SpriteNode.Sprite.Stop();
        }
        internal void Acquire()
        {
            Debug.Assert(!_acquired);
            _acquired = true;
        }
        public void Return()
        {
            Debug.Assert(_acquired);
            _parent.Return(this);
            _acquired = false;
        }
        public void Update()
        {
            Debug.Assert(_acquired);
            Debug.Assert(_animationId != null);
            _animationNode.SpriteNode.Sprite.Update();
        }
        public void Draw()
        {
            Debug.Assert(_acquired);
            Debug.Assert(_animationId != null);
            _animationNode.SpriteNode.Sprite.Draw();
        }
    }
    public class AnimationGenerator
    {
        private Queue<AnimationManager> _managerPool = new();
        private Dictionary<int, SpriteConfigNode> _spriteConfigNodes = [];
        private Dictionary<int, AnimationConfigNode> _animationConfigNodes = [];
        private States _state = States.Configuring;
        internal record SpriteConfigNode(string AssetName, Size RegionSize);
        internal record AnimationConfigNode(int SpriteId, int SpriteAnimationId, int[] Indices, float Period = 0, bool Repeat = false);
        internal Dictionary<int, SpriteConfigNode> SpriteConfigNodes =>  _spriteConfigNodes;
        internal Dictionary<int, AnimationConfigNode> AnimationConfigNodes => _animationConfigNodes;
        public enum States { Configuring, Running }
        public void ConfigureSprite(int spriteId, string assetName, Size regionSize)
        {
            Debug.Assert(_state == States.Configuring);
            Debug.Assert(!_spriteConfigNodes.ContainsKey(spriteId));
            _spriteConfigNodes.Add(spriteId, new(assetName, regionSize));
        }
        public void ConfigureAnimation(int animationId, int spriteId, int spriteAnimationId, int[] indices, float period = 0, bool repeat = false)
        {
            Debug.Assert(_state == States.Configuring);
            Debug.Assert(!_animationConfigNodes.ContainsKey(animationId));
            Debug.Assert(_spriteConfigNodes.ContainsKey(spriteId));
            _animationConfigNodes.Add(animationId, new(spriteId, spriteAnimationId, indices, period, repeat));
        }
        public void Initialize(int capacity = 64)
        {
            Debug.Assert(_state == States.Configuring);
            // Initialize pool
            for (var i = 0; i < capacity; i++)
                _managerPool.Enqueue(new(this));
            // Load all the animations
            {
                var manager = new AnimationManager(this);
                foreach (var animationId in _animationConfigNodes.Keys)
                    manager.LoadAnimation(animationId);
            }
            _state = States.Running;
        }
        public AnimationManager Acquire()
        {
            Debug.Assert(_state == States.Running);
            var manager = _managerPool.Dequeue();
            manager.Acquire();
            return manager;
        }
        internal void Return(AnimationManager manager)
        {
            Debug.Assert(_state == States.Running);
            _managerPool.Enqueue(manager);
        }
    }
    public class Sprite
    {
        private readonly Texture2D _texture;
        private readonly Rectangle[] _regions;
        private readonly Dictionary<int, Node> _nodes = [];
        private Node _node;
        private float _time;
        private int _frame, _index;
        private bool _running = false;
        private Vector2 _origin;
        private record Node(int[] Indices, float Period, bool Repeat);
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
        public void Configure(int id, int[] indices, float period = 0, bool repeat = false)
        {
            Debug.Assert(indices.All(x => x >= 0 && x < _regions.Length));
            Debug.Assert(indices.Length > 0);
            Debug.Assert(period >= 0);
            Debug.Assert(!_nodes.ContainsKey(id));
            _nodes.Add(id, new Node(indices, period, repeat));
        }
        public void Play(int id)
        {
            Debug.Assert(_nodes.ContainsKey(id));
            _node = _nodes[id];
            _time = _node.Period;
            _frame = 0;
            _index = _node.Indices[_frame];
            _running = true;
        }
        public void Stop()
        {
            _running = false;
        }
        public bool Running => _running;
        public Texture2D Texture => _texture;
        public ref Rectangle Region => ref _regions[_index];
        public int Index => _index;
        public int Frame => _frame;
        public ref Vector2 Origin => ref _origin;
        public Color Color = Color.White;
        public Vector2 Position;
        public float Scale = 1;
        public float Rotation = 0;
        public SpriteEffects SpriteEffect = SpriteEffects.None;
        public void Update()
        {
            if (_running && _node.Period > 0)
            {
                while (_time <= 0)
                {
                    if (_frame == _node.Indices.Length - 1)
                    {
                        if (_node.Repeat)
                        {
                            _frame = 0;
                            _index = _node.Indices[_frame];
                        }
                        else
                        {
                            _running = false;
                        }
                    }
                    else
                    {
                        _frame++;
                        _index = _node.Indices[_frame];
                    }
                    _time += _node.Period;
                }

                _time -= Globals.GameTime.GetElapsedSeconds();
            }
        }
        public void Draw()
        {
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
    }
}
