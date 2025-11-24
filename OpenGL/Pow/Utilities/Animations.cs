using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pow.Utilities.GO;
using System.Collections.ObjectModel;
using Pow.Utilities.Shaders;


namespace Pow.Utilities.Animations
{
    public class AnimationManager : IGOManager, IParent
    {
        private bool _acquired = false;
        private readonly AnimationGenerator _parent;
        private readonly Dictionary<int, SpriteNode> _spriteNodes = [];
        private readonly Dictionary<int, AnimationNode> _animationNodes = [];
        private readonly List<IFeature> _features = [];
        private readonly ListEnumerable<IFeature> _featuresEnumerable;
        private AnimationNode _animationNode;
        private int? _animationId = null;
        private Vector2 _position;
        private float _rotation;
        private Directions _direction;
        private Vector2 _scale;
        private Layers _layer;
        private PositionModes _positionMode;
        private Color _color;
        private BlendState _blendState;
        private float _visibility;
        private bool _pauseable;
        private bool _show;
        private Sprite.ServiceFrameUpdatedDelegate _serviceFrameUpdated;
        private record SpriteNode(Sprite Sprite, ReadOnlyDictionary<Directions, SpriteEffects> DirectionSpriteEffects);
        private record AnimationNode(SpriteNode SpriteNode, int SpriteAnimationId);
        private void UpdateSpriteServiceFrameUpdated()
        {
            Debug.Assert(_animationId != null);
            var sprite = _animationNode.SpriteNode.Sprite;
            sprite.ServiceFrameUpdated = _serviceFrameUpdated;
        }
        private void UpdateSpritePosition()
        {
            Debug.Assert(_animationId != null);
            var sprite = _animationNode.SpriteNode.Sprite;
            var offset = Vector2.Zero;
            foreach (var feature in _features)
                offset += feature.DrawOffset;
            sprite.Position = _position + offset;
        }
        private void UpdateSpriteRotation()
        {
            Debug.Assert(_animationId != null);
            var sprite = _animationNode.SpriteNode.Sprite;
            var offset = 0.0f;
            foreach (var feature in _features)
                offset += feature.Rotation;
            sprite.Rotation = MathHelper.WrapAngle(_rotation + offset);
        }
        private void UpdateSpriteSpriteEffect()
        {
            Debug.Assert(_animationId != null);
            var spriteNode = _animationNode.SpriteNode;
            var sprite = spriteNode.Sprite;
            sprite.SpriteEffect = spriteNode.DirectionSpriteEffects[_direction];
        }
        private void UpdateSpriteScale()
        {
            Debug.Assert(_animationId != null);
            var spriteNode = _animationNode.SpriteNode;
            var sprite = spriteNode.Sprite;
            var scale = 1.0f;
            foreach (var feature in _features)
                scale *= feature.Scale;
            sprite.Scale = _scale * scale;
        }
        private void UpdateSpriteColor()
        {
            Debug.Assert(_animationId != null);
            var spriteNode = _animationNode.SpriteNode;
            var sprite = spriteNode.Sprite;
            var visibility = 1.0f;
            foreach (var feature in _features)
                visibility *= feature.Visibility;
            sprite.Color = _color * _visibility * visibility;
        }
        public AnimationManager(AnimationGenerator parent)
        {
            _parent = parent;
            _featuresEnumerable = new(_features);
        }
        public ListEnumerable<IFeature> Features => _featuresEnumerable;
        public int AnimationId
        {
            get
            {
                Debug.Assert(_animationId != null);
                return _animationId.Value;
            }
        }
        public int Index
        {
            get
            {
                Debug.Assert(_animationId != null);
                return _animationNode.SpriteNode.Sprite.Index;
            }
        }
        public int Frame
        {
            get
            {
                Debug.Assert(_animationId != null);
                return _animationNode.SpriteNode.Sprite.Frame;
            }
        }
        public Vector2 Origin
        {
            get
            {
                Debug.Assert(_animationId != null);
                return _animationNode.SpriteNode.Sprite.Origin;
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
        public Sprite.ServiceFrameUpdatedDelegate ServiceFrameUpdated
        {
            get => _serviceFrameUpdated;
            set
            {
                Debug.Assert(_animationId != null);
                if (_serviceFrameUpdated == value) return;
                _serviceFrameUpdated = value;
                UpdateSpriteServiceFrameUpdated();
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
        public float Rotation
        {
            get => _rotation;
            set
            {
                Debug.Assert(_animationId != null);
                if (_rotation == value) return; 
                _rotation = value;
                UpdateSpriteRotation();
            }
        }
        public Directions Direction
        {
            get => _direction;
            set
            {
                Debug.Assert(_animationId != null);
                if (_direction == value) return;
                _direction = value;
                UpdateSpriteSpriteEffect();
            }
        }
        public Vector2 Scale
        {
            get => _scale;
            set
            {
                Debug.Assert(_animationId != null);
                if (_scale == value) return;
                _scale = value;
                UpdateSpriteScale();
            }
        }
        public float Visibility
        {
            get => _visibility;
            set
            {
                Debug.Assert(_animationId != null);
                if (_visibility == value) return;
                _visibility = value;
                UpdateSpriteColor();
            }
        }
        public Color Color
        {
            get => _color;
            set
            {
                Debug.Assert(_animationId != null);
                if (_color == value) return;
                _color = value;
                UpdateSpriteColor();
            }
        }
        public RectangleF Bounds
        {
            get
            {
                Debug.Assert(_animationId != null);
                return _animationNode.SpriteNode.Sprite.Bounds;
            }
        }
        public BlendState BlendState
        {
            get => _blendState;
            set
            {
                Debug.Assert(_animationId != null);
                if (_blendState == value) return;
                _blendState = value;
            }
        }
        public Layers Layer { get => _layer; set => _layer = value; }
        public PositionModes PositionMode { get => _positionMode; set => _positionMode = value; }
        public bool Pauseable { get => _pauseable; set => _pauseable = value; }
        public bool Show { get => _show; set => _show = value; }
        public void LoadSprite(int spriteId)
        {
            Debug.Assert(!_spriteNodes.ContainsKey(spriteId));
            Debug.Assert(_parent.SpriteConfigNodes.ContainsKey(spriteId));
            var spriteConfigNode = _parent.SpriteConfigNodes[spriteId];
            var sprite = new Sprite(spriteConfigNode.AssetName, spriteConfigNode.RegionSize);
            var spriteNode = new SpriteNode(sprite, spriteConfigNode.DirectionSpriteEffects);
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
            UpdateSpriteServiceFrameUpdated();
            UpdateSpritePosition();
            UpdateSpriteRotation();
            UpdateSpriteSpriteEffect();
            UpdateSpriteScale();
            UpdateSpriteColor();
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
            _position = Vector2.Zero;
            _rotation = 0;
            _direction = Directions.Left;
            _layer = Layers.Ground;
            _scale = new(1, 1);
            _positionMode = PositionModes.Map;
            _visibility = 1;
            _color = Color.White;
            _pauseable = true;
            _show = true;
            _serviceFrameUpdated = null;
            _blendState = BlendState.AlphaBlend;
            _acquired = true;
        }
        public void Return()
        {
            Debug.Assert(_acquired);
            foreach (var feature in _features)
                feature.Return();
            _features.Clear();
            _parent.Return(this);
            _acquired = false;
        }
        public void Update()
        {
            Debug.Assert(_acquired);
            Debug.Assert(_animationId != null);
            if (_pauseable && Globals.GamePaused) return;
            foreach (var feature in _features) feature.Update();
            _animationNode.SpriteNode.Sprite.Update();
        }
        public void Draw()
        {
            Debug.Assert(_acquired);
            Debug.Assert(_animationId != null);
            _animationNode.SpriteNode.Sprite.Draw();
        }
        public T1 CreateFeature<T1, T2>() 
            where T1 : FeatureManager<T2>, new()
            where T2 : BaseEffect, new()
        {
            Debug.Assert(_acquired);
            T1 feature = Globals.Runner.FeatureGenerator.Acquire<T1, T2>(this);
            _features.Add(feature);
            return feature;
        }
        Texture2D IParent.Texture
        {
            get
            {
                Debug.Assert(_animationId != null);
                return _animationNode.SpriteNode.Sprite.Texture;
            }
        }
        Rectangle IParent.Region
        {
            get
            {
                Debug.Assert(_animationId != null);
                return _animationNode.SpriteNode.Sprite.Region;
            }
        }
        void IParent.UpdateDrawOffset() => UpdateSpritePosition();
        void IParent.UpdateVisibility() => UpdateSpriteColor();
        void IParent.UpdateScale() => UpdateSpriteScale();
        void IParent.UpdateRotation() => UpdateSpriteRotation();
    }
    public class AnimationGenerator : IGOGenerator<AnimationManager>
    {
        private Stack<AnimationManager> _managerPool = new();
        private Dictionary<int, SpriteConfigNode> _spriteConfigNodes = [];
        private Dictionary<int, AnimationConfigNode> _animationConfigNodes = [];
        private bool _initialized = false;
        internal record SpriteConfigNode(string AssetName, Size RegionSize, ReadOnlyDictionary<Directions, SpriteEffects> DirectionSpriteEffects);
        internal record AnimationConfigNode(int SpriteId, int SpriteAnimationId, int[] Indices, float Period = 0, bool Repeat = false);
        internal Dictionary<int, SpriteConfigNode> SpriteConfigNodes =>  _spriteConfigNodes;
        internal Dictionary<int, AnimationConfigNode> AnimationConfigNodes => _animationConfigNodes;
        private void Create() => _managerPool.Push(new(this));
        public void ConfigureSprite(int spriteId, string assetName, Size regionSize, ReadOnlyDictionary<Directions, SpriteEffects> directionSpriteEffects)
        {
            Debug.Assert(!_initialized);
            Debug.Assert(!_spriteConfigNodes.ContainsKey(spriteId));
            _spriteConfigNodes.Add(spriteId, new(assetName, regionSize, directionSpriteEffects));
        }
        public void ConfigureAnimation(int animationId, int spriteId, int spriteAnimationId, int[] indices, float period = 0, bool repeat = false)
        {
            Debug.Assert(!_initialized);
            Debug.Assert(!_animationConfigNodes.ContainsKey(animationId));
            Debug.Assert(_spriteConfigNodes.ContainsKey(spriteId));
            _animationConfigNodes.Add(animationId, new(spriteId, spriteAnimationId, indices, period, repeat));
        }
        public void Initialize(int capacity = 64)
        {
            Debug.Assert(!_initialized);
            // Initialize pool
            for (var i = 0; i < capacity; i++) Create();
            // Load all the animations
            {
                var manager = new AnimationManager(this);
                foreach (var animationId in _animationConfigNodes.Keys)
                    manager.LoadAnimation(animationId);
            }
            _initialized = true;
        }
        public AnimationManager Acquire()
        {
            Debug.Assert(_initialized);
            if (_managerPool.Count == 0) Create();
            var manager = _managerPool.Pop();
            manager.Acquire();
            return manager;
        }
        internal void Return(AnimationManager manager)
        {
            Debug.Assert(_initialized);
            _managerPool.Push(manager);
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
        private RectangleF _bounds;
        private Vector2 _position;
        private record Node(int[] Indices, float Period, bool Repeat);
        public delegate void ServiceFrameUpdatedDelegate();
        public Sprite(string assetName, Size regionSize)
        {
            _texture = Globals.Game.Content.Load<Texture2D>(assetName);
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
            _bounds = new(
                x: -_origin.X,
                y: -_origin.Y,
                width: regionSize.Width,
                height: regionSize.Height);
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
            ServiceFrameUpdated?.Invoke();
            _running = _node.Period > 0;
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
        public Vector2 Position
        {
            get => _position;
            set
            {
                if (_position == value)
                    return;
                _position = value;
                _bounds.Position = value - _origin;
            }
        }
        public ref RectangleF Bounds => ref _bounds;
        public Vector2 Scale = new(1, 1);
        public float Rotation = 0;
        public SpriteEffects SpriteEffect = SpriteEffects.None;
        public ServiceFrameUpdatedDelegate ServiceFrameUpdated = null;
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
                    ServiceFrameUpdated?.Invoke();
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
