using MonoGame.Extended;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

namespace Duo.Managers.Event;

internal class Submitter : Environment
{
    public enum Triggers { Proximity }
    private string _runnerID;
    private Runner _runner;
    private string _triggerCharacterID;
    private Character _triggerCharacter;
    private float _triggerDistance;
    private string _commandsRaw;
    private List<Runner.Node> _runnerNodes = [];
    public bool Initialized { get; private set; }
    public Triggers Trigger { get; private set; }
    public Vector2 Position { get; private set; }
    public bool Triggered { get; private set; }
    public override void Initialize(PolygonNode node)
    {
        base.Initialize(node);
        _runnerID = node.Parameters.GetValueOrDefault("RunnerID", "Runner");
        _runner = null;
        _triggerCharacterID = node.Parameters.GetValueOrDefault("TriggerCharacterID", "Cat");
        _triggerCharacter = null;
        _triggerDistance = float.Parse(node.Parameters.GetValueOrDefault("TriggerDistance", "32.0"));
        _commandsRaw = node.Parameters.GetValueOrDefault("Commands", "");
        _runnerNodes.Clear();
        Position = node.Position + node.Vertices.Average();
        Trigger = Enum.Parse<Triggers>(node.Parameters.GetValueOrDefault("Trigger", "Proximity"));
        Triggered = false;
        Initialized = false;
    }
    public override void Update()
    {
        base.Update();

        // Initialze the submitter.
        if (!Initialized)
        {
            // Acquire reference to the event runner and create the runner nodes.
            // Normally, we want to avoid allocation to the heap, but this doesn't run every update.
            if (_runner == null)
            {
                Debug.Assert(_runnerNodes.Count == 0);
                _runner = Globals.DuoRunner.Environments.Where(x => x.ID == _runnerID).OfType<Runner>().First();
                _runnerNodes.AddRange(_commandsRaw
                    .Split(";")
                    .Select(command => command
                        .Split("#")
                        .ToDictionary(pair => pair.Split(":")[0].Trim(), pair => pair.Split(":")[1].Trim()))
                    .Select(command => new Runner.Node(
                        action: Enum.Parse<Runner.Actions>(command["Action"]),
                        startCondition: Enum.Parse<Runner.Triggers>(command["StartCondition"]),
                        stopCondition: Enum.Parse<Runner.Triggers>(command["StopCondition"]),
                        dialogueMessage: command.GetValueOrDefault("DialogueMessage", null),
                        timeOutValue: float.Parse(command.GetValueOrDefault("TimeOutValue", "1.0")))));
            }

            // Acquire reference to the trigger character, given the appropriate trigger.
            if (Trigger == Triggers.Proximity && _triggerCharacter == null)
            {
                _triggerCharacter = Globals.DuoRunner.Environments.Where(x => x.ID == _triggerCharacterID).OfType<Character>().First();
            }

            // Once all the required references have been found, the submitter is initialized.
            if ((_runner != null && _runner.Initialized) && 
                (Trigger != Triggers.Proximity || _triggerCharacter != null))
            {
                Initialized = true;
            }
        }

        // Main loop.
        else
        {
            // Submit the commands
            if (!Triggered && 
                (Trigger == Triggers.Proximity && ((Position - _triggerCharacter.Position).LengthSquared() <= (_triggerDistance * _triggerDistance))))
            {
                foreach (var node in _runnerNodes)
                    _runner.Submit(node);
                Triggered = true;
            }
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
        float timeOutValue = 1.0f) : INode
    {
        private float _time = timeOutValue;
        public Actions Action { get; } = action;
        public Triggers StartCondition { get; } = startCondition;
        public Triggers StopCondition { get; } = stopCondition;
        public string DialogueMessage { get; } = dialogueMessage;
        public float TimeOutValue { get; } = timeOutValue;
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
    public void Submit(Node node)
    {
        Debug.Assert(!node.Stopped);
        Debug.Assert(!node.Running);
        Debug.Assert(_startConditions.Contains(node.StartCondition));
        Debug.Assert(_stopConditions.Contains(node.StopCondition));
        if (node.Action == Actions.Dialogue)
            Debug.Assert(node.DialogueMessage != null);
        if (node.StopCondition == Triggers.Timeout)
            Debug.Assert(node.TimeOutValue > 0);
        _waitingNodes.Enqueue(node);
    }
    public Node Submit(
        Actions action,
        Triggers startCondition,
        Triggers stopCondition,
        string dialogueMessage = null,
        float timeOutValue = 1.0f)
    {
        // In general, we don't want to allocate to heap,
        // however this is an exemption because submit calls should be sparse.
        var node = new Node(
            action: action,
            startCondition: startCondition,
            stopCondition: stopCondition,
            dialogueMessage: dialogueMessage,
            timeOutValue: timeOutValue);
        Submit(node);
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

                // Queue up the node for the start up for the given conditions.
                if ((node.StartCondition == Triggers.Immediate) ||
                    (node.StartCondition == Triggers.NoOutstanding && _outstandingNodes.Count == 0 && _startNodes.Count == 0))
                {
                    _startNodes.Enqueue(node);
                    _waitingNodes.Dequeue();
                }

                // If the latest waiting node needs to block due to existing outstanding, stop trying to queue up nodes.
                if (node.StartCondition == Triggers.NoOutstanding && (_outstandingNodes.Count > 0 || _startNodes.Count > 0))
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

                // Update the node.
                inode.Update();

                // Perform stop condition for dialogue action.
                if (node.Action == Actions.Dialogue)
                {
                    var dialogueNode = inode.DialogueNode;

                    if (node.StopCondition == Triggers.Timeout)
                    {
                        if (!dialogueNode.Closed && node.TimeOutFinished)
                            dialogueNode.Close();
                        if (dialogueNode.Closed)
                            _stopNodes.Enqueue(node);
                    }
                }
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
