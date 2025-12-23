using MonoGame.Extended;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers.Event;

internal class Submitter : Environment
{
    public enum Triggers { Proximity }
    private string runnerID;
    private Runner runner;
    private string triggerCharacterID;
    private Character triggerCharacter;
    public bool Initialized { get; private set; }
    public Triggers Trigger { get; private set; }
    public override void Initialize(PolygonNode node)
    {
        base.Initialize(node);
        runnerID = node.Parameters.GetValueOrDefault("RunnerID", "Runner");
        runner = null;
        triggerCharacterID = node.Parameters.GetValueOrDefault("TriggerCharacterID", "Cat");
        triggerCharacter = null;
        Trigger = Enum.Parse<Triggers>(node.Parameters.GetValueOrDefault("Trigger", "Proximity"));
        Initialized = false;
    }
    public override void Update()
    {
        base.Update();
        if (!Initialized)
        {
        }
        else
        {

        }
    }
}


internal class Runner : Environment
{
    public enum Actions { Dialogue }
    public enum Triggers { Immediate, NoOutstanding, Timeout }
    private interface INode
    {
        public Dialogue.Node DialogueNode { get; set; }
        public void Update();
        public void Start();
        public void Stop();
    }
    public class Node(
        Actions action,
        Triggers startCondition,
        Triggers stopCondition,
        string dialogueMessage = null,
        float? timeOutValue = null) : INode
    {
        private float _time = timeOutValue.GetValueOrDefault();
        public Actions Action { get; } = action;
        public Triggers StartCondition { get; } = startCondition;
        public Triggers StopCondition { get; } = stopCondition;
        public string DialogueMessage { get; } = dialogueMessage;
        public float? TimeOutValue { get; } = timeOutValue;
        public bool TimeOutFinished => _time <= 0;
        public bool Running { get; private set; } = false;
        public bool Stopped { get; private set; } = false;
        void INode.Update()
        {
            Debug.Assert(Running);

            // Update timeout.
            if (StopCondition == Triggers.Timeout && _time > 0)
                _time -= Pow.Globals.GameTime.GetElapsedSeconds();
        }
        Dialogue.Node INode.DialogueNode { get; set; } = null;
        void INode.Start()
        {
            // Check to make sure the state of everything is what we expect.
            Debug.Assert(!Stopped);
            Debug.Assert(!Running);
            var node = (INode)this;
            if (Action == Actions.Dialogue)
                Debug.Assert(node.DialogueNode != null);
            // Set the running state.
            Running = true;
        }
        void INode.Stop()
        {
            Running = false;
            Stopped = true;
        }
    }
    private static readonly Triggers[] _startConditions = [Triggers.Immediate, Triggers.NoOutstanding];
    private static readonly Triggers[] _stopConditions = [Triggers.Timeout];
    private Queue<Node> _waitingNodes = new();
    private List<Node> _outstandingNodes = new();
    private Queue<Node> _startNodes = new();
    private Queue<Node> _stopNodes = new();
    private string _dialogueID;
    private Dialogue _dialogue;
    public bool Initialized { get; private set; }
    public Node Submit(
        Actions action,
        Triggers startCondition,
        Triggers stopCondition,
        string dialogueMessage = null,
        float? timeOutValue = null)
    {
        Debug.Assert(_startConditions.Contains(startCondition));
        Debug.Assert(_stopConditions.Contains(stopCondition));
        if (action == Actions.Dialogue)
            Debug.Assert(dialogueMessage != null);
        if (stopCondition == Triggers.Timeout)
        {
            Debug.Assert(timeOutValue.HasValue);
            Debug.Assert(timeOutValue > 0);
        }
        var node = new Node(
            action: action,
            startCondition: startCondition,
            stopCondition: stopCondition,
            dialogueMessage: dialogueMessage,
            timeOutValue: timeOutValue);
        _waitingNodes.Enqueue(node);
        return node;
    }
    public override void Initialize(PolygonNode node)
    {
        base.Initialize(node);
        _dialogueID = node.Parameters.GetValueOrDefault("DialogueID", "Dialogue");
        _dialogue = null;
        _waitingNodes.Clear();
        _outstandingNodes.Clear();
        _startNodes.Clear();
        _stopNodes.Clear();
        Initialized = false;
    }
    public override void Update()
    {
        base.Update();

        // Don't perform updates if the game is paused.
        if (Pow.Globals.GamePaused)
            return;

        // Initialization needs to occur first.
        if (!Initialized)
        {
            if (_dialogue == null)
            {
                Debug.Assert(!Initialized);
                _dialogue = Globals.DuoRunner.Environments.Where(x => x.ID == _dialogueID).OfType<Dialogue>().First();
            }

            if (_dialogue != null)
                Initialized = true;
        }

        // Main loop below.
        else
        {
            Debug.Assert(_startNodes.Count == 0);
            Debug.Assert(_stopNodes.Count == 0);

            // Queue up the next waiting nodes if their start conditions are met.
            while (_waitingNodes.Count > 0)
            {
                var node = _waitingNodes.Peek();

                if ((node.StartCondition == Triggers.Immediate) ||
                    (node.StartCondition == Triggers.NoOutstanding && _outstandingNodes.Count == 0))
                {
                    _startNodes.Enqueue(node);
                    _waitingNodes.Dequeue();
                }

                if (node.StartCondition == Triggers.NoOutstanding && _outstandingNodes.Count > 0)
                    break;
            }

            // Start the queued nodes.
            while (_startNodes.Count > 0)
            {
                // Pop the start noding and verify its state.
                var node = _startNodes.Dequeue();
                Debug.Assert(!node.Running && !node.Stopped);

                var inode = (INode)node;

                // Prepare node if it's a dialogue action.
                // Messages need to be submitted to the dialogue manager.
                if (node.Action == Actions.Dialogue)
                    inode.DialogueNode = _dialogue.Submit(message: node.DialogueMessage);

                // Start the node and add it to the outstanding queue.
                inode.Start();
                _outstandingNodes.Add(node);
            }

            // Update the outstanding nodes.
            // Also check for stop conditions.
            foreach (var node in _outstandingNodes)
            {
                Debug.Assert(node.Running && !node.Stopped);
                var inode = (INode)node;
                inode.Update();

                if (node.StopCondition == Triggers.Timeout && node.TimeOutFinished)
                    _stopNodes.Enqueue(node);
            }

            // Queued nodes are stopped and removed from the outstanding list.
            while (_stopNodes.Count > 0)
            {
                var node = _stopNodes.Dequeue();
                Debug.Assert(node.Running && !node.Stopped);
                var inode = (INode)node;
                inode.Stop();
                _outstandingNodes.Remove(node);
            }
        }
    }
}
