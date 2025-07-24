using MonoGame.Extended;
using Pow.Utilities.UA;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Utilities.Animations
{
    public interface IAnimationGroup
    {
        public void Initialize(AnimationManager manager);
        public void Play();
        public void Stop();
        public bool Running { get; }
        public void Update();
    }
    public class PlaySingleGroup(int animationId) : IAnimationGroup
    {
        private readonly int _animationId = animationId;
        private AnimationManager _manager;
        private bool _initialized = false;
        public bool Running
        {
            get
            {
                Debug.Assert(_initialized);
                return _manager.Running;
            }
        }

        public void Initialize(AnimationManager manager)
        {
            Debug.Assert(!_initialized);
            _manager = manager;
            _initialized = true;
        }

        public void Play()
        {
            Debug.Assert(_initialized);
            _manager.Play(_animationId);
        }

        public void Stop()
        {
            Debug.Assert(_initialized);
            _manager.Stop();
        }

        public void Update()
        {
            Debug.Assert(_initialized);
        }
    }
    public class PlayIdleGroup : IAnimationGroup
    {
        private readonly static Random _random = new Random();
        private readonly int _primaryAnimationId;
        private readonly int[] _secondaryAnimationIds;
        private readonly float _waitMult, _waitOff;
        private bool _initialized = false;
        private bool _running = false;
        private bool _primaryActive = false;
        private float _wait;
        private AnimationManager _manager;
        public PlayIdleGroup(int primaryAnimationId, float minWait, float maxWait, int[] secondaryAnimationIds)
        {
            Debug.Assert(primaryAnimationId >= 0);
            Debug.Assert(minWait > 0 && minWait <= maxWait);
            Debug.Assert(secondaryAnimationIds.All(x=>x >= 0));
            _primaryAnimationId = primaryAnimationId;
            _secondaryAnimationIds = secondaryAnimationIds;
            _waitOff = minWait;
            _waitMult = maxWait - minWait;
        }
        public void Initialize(AnimationManager manager)
        {
            Debug.Assert(!_initialized);
            _manager = manager;
            _initialized = true;
        }
        public bool Running
        {
            get
            {
                Debug.Assert(_initialized);
                return _running;
            }
        }
        public void Play()
        {
            Debug.Assert(_initialized);
            _manager.Play(_primaryAnimationId);
            _primaryActive = true;
            _wait = _waitOff + _random.NextSingle() * _waitMult;
            _running = true;
        }
        public void Stop()
        {
            Debug.Assert(_initialized);
            _manager.Stop();
            _primaryActive = true;
            _wait = 0;
            _running = false;
        }
        public void Update()
        {
            Debug.Assert(_initialized);

            if (!_running)
                return;

            if (_primaryActive && _wait <= 0)
            {
                _primaryActive = false;
                _manager.Play(_secondaryAnimationIds[_random.Next(_secondaryAnimationIds.Length)]);
            }
            else if (!_primaryActive && !_manager.Running)
            {
                _primaryActive = true;
                _wait = _waitOff + _random.NextSingle() * _waitMult;
                _manager.Play(_primaryAnimationId);
            }

            var timeElapsed = Globals.GameTime.GetElapsedSeconds();
            if (_wait > 0)
                _wait -= timeElapsed;
        }
    }
    public class AnimationGroupManager(AnimationManager manager)
    {
        private bool _initialized = false;
        private int _groupId = -1;
        private readonly Dictionary<int, IAnimationGroup> _groups = [];
        private readonly AnimationManager _manager = manager;
        public void Configure(int groupId, IAnimationGroup group)
        {
            Debug.Assert(!_initialized);
            Debug.Assert(!_groups.ContainsKey(groupId));
            group.Initialize(_manager);
            _groups[groupId] = group;
        }
        public bool Running
        {
            get
            {
                Debug.Assert(_initialized);
                return _groupId >= 0 && _groups[_groupId].Running;
            }
        }
        public void Initialize()
        {
            Debug.Assert(!_initialized);
            _initialized = true;
        }
        public void Play(int groupId)
        {
            Debug.Assert(_initialized);
            Debug.Assert(_groups.ContainsKey(groupId));
            Stop();
            _groupId = groupId;
            _groups[groupId].Play();
        }
        public void Stop()
        {
            Debug.Assert(_initialized);
            if (_groupId < 0 || !_groups[_groupId].Running)
                return;
            _groups[_groupId].Stop();
        }
        public void Update()
        {
            Debug.Assert(_initialized);
            if (_groupId < 0)
                return;
            _groups[_groupId].Update();
        }
    }
}
