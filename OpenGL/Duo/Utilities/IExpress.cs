using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Utilities;

internal interface IExpress
{
    public class Node(string expression)
    {
        public string Expression { get; } = expression;
        public bool Running { get; private set; } = false;
        public bool Stopped { get; private set; } = false;
        public bool Closed { get; private set; } = false;
        public void Close()
        {
            Debug.Assert(!Stopped);
            Closed = true;
        }
        public void Start()
        {
            Debug.Assert(!Stopped);
            Debug.Assert(!Running);
            Running = true;
        }
        public void Stop()
        {
            Debug.Assert(Running);
            Debug.Assert(!Stopped);
            Running = false;
            Stopped = true;
        }
    }
    public Node Submit(string expression);
}
