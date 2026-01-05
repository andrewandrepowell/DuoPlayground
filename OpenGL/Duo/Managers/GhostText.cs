using DuoGum.Components;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers;

internal class GhostText : GumObject
{
    private const int _maxNodes = 64;
    private Queue<Node> _nodes = new();
    private ghostTextView _view;
    private interface INode
    {
        public void Start();
        public void Stop();
    }
    public class Node(string message) : INode
    {
        public readonly string Message = message;
        public bool Running
        {
            get;
            private set;
        } = false;
        public bool Closed
        {
            get;
            private set;
        } = false;
        public void Close()
        {
            Closed = true;
        }
        void INode.Start()
        {
            Debug.Assert(!Running);
            Running = true;
        }
        void INode.Stop()
        {
            Debug.Assert(Running);
            Running = false;
        }
    }
    public override void Initialize(PolygonNode node)
    {
        base.Initialize(node);
        _view = new ghostTextView();
        GumManager.Initialize(_view.Visual);
        GumManager.Layer = Layers.Interface;
        GumManager.PositionMode = PositionModes.Screen;
        GumManager.Position = GumManager.Origin;
    }
    public override void Update()
    {
        base.Update();
    }
}
