using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers;

internal partial class Character
{
    private const int _maxNumberOfExpressNodes = 32;
    private interface IExpress
    {
        public void Start();
        public void Stop();
    }
    public class ExpressNode(int id) : IExpress
    {
        public int ID { get; } = id;
        public bool Running { get; private set; } = false;
        public bool Stopped { get; private set; } = false;
        public bool Closed { get; private set; } = false;
        public void Close()
        {
            Debug.Assert(!Stopped);
            Closed = true;
        }
        void IExpress.Start()
        {
            Debug.Assert(!Stopped);
            Debug.Assert(!Running);
            Running = true;
        }
        void IExpress.Stop()
        {
            Debug.Assert(Running);
            Debug.Assert(!Stopped);
            Running = false;
            Stopped = true;
        }
    }
    private Queue<ExpressNode> _expressNodes = [];
    private void InitializeExpress()
    {
        _expressNodes.Clear();
    }
    private void CleanupExpress()
    {
        _expressNodes.Clear();
    }
    private void UpdateExpress(int id)
    {
        Debug.Assert(Action == Actions.Express);
        if (ExpressAnimationMap.TryGetValue(id, out var animation))
        {
            var animationManager = AnimationManager;
            animationManager.Play((int)animation);
        }
    }
    private void ExpressUpdate()
    {
        if (Pow.Globals.GamePaused) 
            return;

        if (_expressNodes.TryPeek(out var node))
        {
            var inode = (IExpress)node;
            if (!node.Running && !node.Stopped && !node.Closed)
            {
                inode.Start();
                UpdateExpress(node.ID);
            }
            if (node.Running && !node.Stopped && node.Closed)
                inode.Stop();
            if (!node.Running && (node.Stopped || node.Closed))
                _expressNodes.Dequeue();
        }
    }
    public ExpressNode SubmitExpress(int id)
    {
        Debug.Assert(_expressNodes.Count <= _maxNumberOfExpressNodes);
        var node = new ExpressNode(id: id);
        _expressNodes.Enqueue(node);
        return node;
    }
}
