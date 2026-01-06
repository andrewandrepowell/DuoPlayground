using DuoGum.Components;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
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
    private const float _stateChangePeriod = 0.5f;
    private float _stateTime;
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
    private void Open()
    {
        // Verify in appropriate state before proceeding.
        Debug.Assert(State == RunningStates.Waiting);
        Debug.Assert(_nodes.Count > 0);
        var node = _nodes.Peek();
        Debug.Assert(!node.Running);
        var inode = (INode)node;
        // Configure message.
        _view.ghostTextBoxInstance.GhostTextText = node.Message;
        inode.Start();
        // Prepare for state change.
        GumManager.Visibility = 0.0f;
        _stateTime = _stateChangePeriod;
        State = RunningStates.Starting;
    }
    private void Close()
    {
        // Verify in appropriate state before proceeding.
        Debug.Assert(State == RunningStates.Running);
        Debug.Assert(_nodes.Count > 0);
        var node = _nodes.Peek();
        Debug.Assert(node.Running);
        Debug.Assert(node.Closed);
        var inode = (INode)node;
        // Prepare for state change.
        GumManager.Visibility = 1.0f;
        _stateTime = _stateChangePeriod;
        State = RunningStates.Stopping;
    }
    private void ForceOpen()
    {
        // Verify in appropriate state before proceeding.
        Debug.Assert(State == RunningStates.Starting);
        Debug.Assert(_nodes.Count > 0);
        var node = _nodes.Peek();
        Debug.Assert(node.Running);
        // Prepare for state change.
        GumManager.Visibility = 1.0f;
        State = RunningStates.Running;
    }
    private void ForceClose()
    {
        // Verify in appropriate state before proceeding.
        Debug.Assert(State == RunningStates.Stopping);
        Debug.Assert(_nodes.Count > 0);
        var node = _nodes.Peek();
        Debug.Assert(node.Running);
        Debug.Assert(node.Closed);
        var inode = (INode)node;
        // Stop the node to indicate back to use this specific message is complete.
        inode.Stop();
        // Prepare for state change.
        GumManager.Visibility = 0.0f;
        State = RunningStates.Waiting;
    }
    public RunningStates State
    {
        get;
        private set;
    } = RunningStates.Waiting;
    public Node Submit(string message)
    {
        // In general, we don't want to allocate to heap,
        // however this is an exemption because submit calls should be sparse.
        var node = new Node(message: message);
        Debug.Assert(_nodes.Count <= _maxNodes);
        _nodes.Enqueue(node);
        return node;
    }
    public override void Initialize(PolygonNode node)
    {
        base.Initialize(node);
        // Initialize the gum manager.
        _view = new ghostTextView();
        GumManager.Initialize(_view.Visual);
        GumManager.Layer = Layers.Interface;
        GumManager.PositionMode = PositionModes.Screen;
        GumManager.Position = GumManager.Origin;
        // Initialize the state of the ghost text.
        GumManager.Visibility = 0.0f;
        State = RunningStates.Waiting;
    }
    public override void Update()
    {
        base.Update();

        // Don't update if paused.
        if (Pow.Globals.GamePaused) return;

        // Update visibility based on state.
        if (State == RunningStates.Starting && _stateTime >= 0)
            GumManager.Visibility = MathHelper.Lerp(1, 0, _stateTime / _stateTime);
        if (State == RunningStates.Stopping && _stateTime >= 0)
            GumManager.Visibility = MathHelper.Lerp(0, 1, _stateTime / _stateTime);

        // Update state.
        // The State represents whether the dialogue box is open or not.
        if (State == RunningStates.Waiting && _nodes.Count > 0)
            Open();
        if (State == RunningStates.Running && _nodes.Peek().Closed)
            Close();
        if (State == RunningStates.Starting && _stateTime <= 0)
            ForceOpen();
        if (State == RunningStates.Stopping && _stateTime <= 0)
            ForceClose();

        // Update time
        var timeElapsed = Pow.Globals.GameTime.GetElapsedSeconds();
        if (_stateTime > 0)
            _stateTime -= timeElapsed;
    }
}
