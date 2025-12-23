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

internal class Dialogue : GumObject
{
    private const int _maxNodes = 64;
    private readonly static float _closedHeightOffset = Globals.GameWindowSize.Height / 2;
    private Queue<Node> _nodes = new();
    private const float _stateChangePeriod = 0.5f;
    private float _stateTime;
    private const float _updateMessagePeriod = 0.25f;
    private const int _charactersPerUpdateToMessage = 32;
    private float _updateTime;
    private dialogueView _view;
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
            Debug.Assert(Running);
            Running = true;
        }
        void INode.Stop()
        {
            Debug.Assert(!Running);
            Running = false;
        }
    }
    private void Open()
    {
        Debug.Assert(State == RunningStates.Waiting);
        GumManager.Position = new Vector2(x: GumManager.Origin.X, y: GumManager.Origin.Y + _closedHeightOffset);
        _stateTime = _stateChangePeriod;
        State = RunningStates.Starting;
    }
    private void ForceOpen()
    {
        GumManager.Position = GumManager.Origin;
        State = RunningStates.Starting;
    }
    private void Close()
    {
        Debug.Assert(State == RunningStates.Running);
        GumManager.Position = GumManager.Origin;
        _stateTime = _stateChangePeriod;
        State = RunningStates.Starting;
    }
    private void ForceClose()
    {
        GumManager.Position = new Vector2(x: GumManager.Origin.X, y: GumManager.Origin.Y + _closedHeightOffset);
        State = RunningStates.Waiting;
    }
    private void OpenMessage()
    {
        // Verify state of the submitted node.
        Debug.Assert(State == RunningStates.Running);
        Debug.Assert(_nodes.Count > 0);
        var node = _nodes.Peek();
        Debug.Assert(!node.Running);
        Debug.Assert(!node.Closed);
        // Update the dialogue box with the message from the node.
        var dialogueBox = _view.dialogueBoxInstance;
        dialogueBox.DialogueText = node.Message;
        dialogueBox.DialogueMaxLettersToShow = 0;
        // Update the state of the node.
        var privateNode = (INode)node;
        privateNode.Start();
    }
    private void CloseMessage()
    {
        // Verify state of the submitted node.
        Debug.Assert(State == RunningStates.Running);
        Debug.Assert(_nodes.Count > 0);
        var node = _nodes.Peek();
        Debug.Assert(node.Running);
        Debug.Assert(node.Closed);
        // Update the dialogue box so that the message is removed..
        var dialogueBox = _view.dialogueBoxInstance;
        dialogueBox.DialogueText = "";
        // Update the state of the node.
        var privateNode = (INode)node;
        privateNode.Stop();
        // Remove the node from the queue.
        _nodes.Dequeue();
    }
    private void UpdateMessage()
    {
        // Verify state of the submitted node.
        Debug.Assert(State == RunningStates.Running);
        Debug.Assert(_nodes.Count > 0);
        var node = _nodes.Peek();
        Debug.Assert(node.Running);
        Debug.Assert(!node.Closed);
        var dialogueBox = _view.dialogueBoxInstance;
        Debug.Assert(dialogueBox.DialogueMaxLettersToShow != null);
        // Update the number of letters to show.
        var newLettersToShow = dialogueBox.DialogueMaxLettersToShow + _charactersPerUpdateToMessage;
        if (newLettersToShow < dialogueBox.DialogueText.Length)
            dialogueBox.DialogueMaxLettersToShow = newLettersToShow;
        else
            dialogueBox.DialogueMaxLettersToShow = null;
        // Set the update time.
        _updateTime = _updateMessagePeriod;
    }
    public RunningStates State
    {
        get;
        private set;
    } = RunningStates.Waiting;
    public int Count => _nodes.Count;
    public override void Initialize(PolygonNode node)
    {
        base.Initialize(node);
        {
            _view = new dialogueView();
            GumManager.Initialize(_view.Visual);
            GumManager.Layer = Layers.Interface;
            GumManager.PositionMode = PositionModes.Screen;
        }
        {
            ForceClose();
        }
    }
    public override void Cleanup()
    {
        _nodes.Clear();
        base.Cleanup();
    }
    public Node Submit(string message)
    {
        var node = new Node(message: message);
        Debug.Assert(_nodes.Count <= _maxNodes);
        _nodes.Enqueue(node);
        return node;
    }
    public override void Update()
    {
        base.Update();

        // Don't update if paused.
        if (Pow.Globals.GamePaused) return;

        // Update position of the dialogue window.
        if (State == RunningStates.Running && _stateTime >= 0)
            GumManager.Position = new Vector2(
                x: GumManager.Origin.X,
                y: GumManager.Origin.Y + MathHelper.SmoothStep(0, _closedHeightOffset, _stateTime / _stateChangePeriod));
        if (State == RunningStates.Stopping && _stateTime >= 0)
            GumManager.Position = new Vector2(
                x: GumManager.Origin.X,
                y: GumManager.Origin.Y + MathHelper.SmoothStep(_closedHeightOffset, 0, _stateTime / _stateChangePeriod));

        // Update state.
        // The State represents whether the dialogue box is open or not.
        if (State == RunningStates.Waiting && _nodes.Count > 0)
            Open();
        if (State == RunningStates.Running && _nodes.Count == 0)
            Close();
        if (State == RunningStates.Starting && _stateTime <= 0)
            ForceOpen();
        if (State == RunningStates.Stopping && _stateTime <= 0)
            ForceClose();

        // Update time
        var timeElapsed = Pow.Globals.GameTime.GetElapsedSeconds();
        if (_stateTime > 0)
            _stateTime -= timeElapsed;
        if (_updateTime > 0)
            _updateTime -= timeElapsed;
    }
}
