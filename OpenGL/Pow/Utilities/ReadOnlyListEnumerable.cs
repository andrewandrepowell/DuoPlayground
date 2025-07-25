using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pow.Utilities
{
    public readonly struct ListEnumerable<T>(List<T> list) : IEnumerable<T>
    {
        private readonly List<T>  _list = list;
        public readonly int Count => _list.Count;
        public readonly List<T>.Enumerator GetEnumerator() => _list.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    }
}
