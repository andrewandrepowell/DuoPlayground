using Arch.LowLevel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Utilities
{

    public abstract class Pool<T>  : IDisposable where T : class, new()
    {
        private readonly Resources<T> _resource;
        private readonly UnsafeQueue<Handle<T>> _pool;
        public Pool(int amount = 1)
        {
            Debug.Assert(amount >= 1);
            _resource = new(amount);
            _pool = new(amount);
            for (var i = 0; i < amount; i++)
            {
                var handle = _resource.Add(new T());
                _pool.Enqueue(handle);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Handle<T> Acquire() => _pool.Dequeue();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(Handle<T> handle) => _pool.Enqueue(handle);
        public void Dispose()
        {
            _resource.Dispose();
            _pool.Dispose();
        }
        
    }
}
