using Arch.Core.Extensions;
using Duo.Data;
using Duo.Utilities.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Pow.Components;
using Pow.Utilities;
using Pow.Utilities.Control;
using Pow.Utilities.UA;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

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
                        message: command.GetValueOrDefault("Message", null),
                        timeOutValue: float.Parse(command.GetValueOrDefault("TimeOutValue", "1.0")),
                        duoObject: command.TryGetValue("DuoObjectID", out string duoObjectID) ? Globals.DuoRunner.Environments
                            .Where(x=>x.ID == duoObjectID)
                            .OfType<DuoObject>()
                            .First() : null,
                        cameraMode: Enum.Parse<Camera.Modes>(command.GetValueOrDefault("CameraMode", "FullTrack")),
                        expressID: (command.TryGetValue("CharacterID", out string characterID0) ? Globals.DuoRunner.Environments
                            .Where(x => x.ID == characterID0)
                            .OfType<Character>()
                            .First()
                            .ParseExpress(command.TryGetValue("ExpressID", out var expressID) ? expressID : "Idle") : 0),
                        character: (command.TryGetValue("CharacterID", out string characterID1) ? Globals.DuoRunner.Environments
                            .Where(x=>x.ID == characterID1)
                            .OfType<Character>()
                            .First() : null))));
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


internal class Runner : Environment, IUserAction, IControl
{
    public enum Actions { Dialogue, GhostText, Camera, Express }
    public enum Triggers { Immediate, NoOutstanding, Timeout, NotRunning, Interacted }
    private interface INode
    {
        public object ControlNode { get; set; }
        public void Update();
        public void Start();
        public void Stop();
    }
    public class Node(
        Actions action,
        Triggers startCondition,
        Triggers stopCondition,
        string message = null,
        float timeOutValue = 1.0f,
        DuoObject duoObject = null,
        Camera.Modes cameraMode = Camera.Modes.FullTrack,
        int expressID = 0,
        Character character = null) : INode
    {
        private float _time = timeOutValue;
        public Actions Action { get; } = action;
        public Triggers StartCondition { get; } = startCondition;
        public Triggers StopCondition { get; } = stopCondition;
        public string Message { get; } = message;
        public float TimeOutValue { get; } = timeOutValue;
        public DuoObject DuoObject { get; } = duoObject;
        public Camera.Modes CameraMode { get; } = cameraMode;
        public int ExpressID { get; } = expressID;
        public Character Character { get; } = character;
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
        object INode.ControlNode { get; set; }
        void INode.Start()
        {
            // Check to make sure the state of everything is what we expect.
            Debug.Assert(!Stopped);
            Debug.Assert(!Running);
            var node = (INode)this;
#if DEBUG
            if (Action == Actions.Dialogue)
                Debug.Assert(node.ControlNode is Dialogue.Node);
            if (Action == Actions.GhostText)
                Debug.Assert(node.ControlNode is GhostText.Node);
            if (Action == Actions.Express)
                Debug.Assert(node.ControlNode is Character.ExpressNode);
#endif
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
    private static readonly Triggers[] _stopConditions = [Triggers.Timeout, Triggers.NotRunning, Triggers.Interacted];
    private Queue<Node> _waitingNodes = new();
    private List<Node> _outstandingNodes = new();
    private Queue<Node> _startNodes = new();
    private Queue<Node> _stopNodes = new();
    private string _dialogueID;
    private Dialogue _dialogue;
    private string _cameraID;
    private Camera _camera;
    private string _ghostTextID;
    private GhostText _ghostText;
    private UAManager _uaManager;
    public bool Initialized { get; private set; }
    public Keys[] ControlKeys => _uaManager.ControlKeys;
    public Buttons[] ControlButtons => _uaManager.ControlButtons;
    public Directions[] ControlThumbsticks => _uaManager.ControlThumbsticks;
    public void UpdateControl(ButtonStates buttonState, Keys key) => _uaManager.UpdateControl(buttonState, key);
    public void UpdateControl(ButtonStates buttonState, Buttons button) => _uaManager.UpdateControl(buttonState, button);
    public void UpdateControl(Directions thumbsticks, Vector2 position) => _uaManager.UpdateControl(thumbsticks, position);
    public void UpdateUserAction(int actionId, ButtonStates buttonState, float strength)
    {
        if (!Initialized)
            return;

        if (Pow.Globals.GamePaused)
            return;

        var control = (Controls)actionId;

        if (buttonState == ButtonStates.Pressed && control == Controls.Interact)
        {
            foreach (var node in _outstandingNodes)
            {
                Debug.Assert(node.Running && !node.Stopped);
                var inode = (INode)node;

                if (node.StopCondition != Triggers.Interacted)
                    continue;

                if (node.Action == Actions.Dialogue)
                {
                    var dialogueNode = (Dialogue.Node)inode.ControlNode;
                    if (!dialogueNode.Closed)
                        dialogueNode.Close();
                }

                if (node.Action == Actions.GhostText)
                {
                    var ghostTextNode = (GhostText.Node)inode.ControlNode;
                    if (!ghostTextNode.Closed)
                        ghostTextNode.Close();
                }

                if (node.Action == Actions.Express)
                {
                    var expressNode = (Character.ExpressNode)inode.ControlNode;
                    if (!expressNode.Closed)
                        expressNode.Close();
                }
            }
        }
    }
    public void Submit(Node node)
    {
        Debug.Assert(!node.Stopped);
        Debug.Assert(!node.Running);
        Debug.Assert(_startConditions.Contains(node.StartCondition));
        Debug.Assert(_stopConditions.Contains(node.StopCondition));
        if (node.Action == Actions.Dialogue)
            Debug.Assert(node.Message != null);
        if (node.StopCondition == Triggers.Timeout)
            Debug.Assert(node.TimeOutValue > 0);
        _waitingNodes.Enqueue(node);
    }
    public override void Initialize(PolygonNode node)
    {
        base.Initialize(node);
        {
            _dialogueID = node.Parameters.GetValueOrDefault("DialogueID", "Dialogue");
            _dialogue = null;
            _cameraID = node.Parameters.GetValueOrDefault("CameraID", "Camera");
            _camera = null;
            _ghostTextID = node.Parameters.GetValueOrDefault("GhostTextID", "GhostText");
            _ghostText = null;
        }
        {
            _waitingNodes.Clear();
            _outstandingNodes.Clear();
            _startNodes.Clear();
            _stopNodes.Clear();
        }
        {
            Entity.Get<ControlComponent>().Manager.Initialize(this);
            _uaManager = Globals.DuoRunner.UAGenerator.Acquire();
            _uaManager.Initialize(this);
        }
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
            _dialogue ??= Globals.DuoRunner.Environments.Where(x => x.ID == _dialogueID).OfType<Dialogue>().First();
            _camera ??= Globals.DuoRunner.Environments.Where(x => x.ID == _cameraID).OfType<Camera>().First();
            _ghostText ??= Globals.DuoRunner.Environments.Where(x => x.ID == _ghostTextID).OfType<GhostText>().First();

            if (_dialogue != null && _camera != null && _ghostText != null)
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
                {
                    Debug.Assert(
                        node.StopCondition == Triggers.Timeout || 
                        node.StopCondition == Triggers.Interacted);
                    inode.ControlNode = _dialogue.Submit(message: node.Message);
                }

                // Prepare node if it's a ghost text action.
                if (node.Action == Actions.GhostText)
                {
                    Debug.Assert(
                        node.StopCondition == Triggers.Timeout ||
                        node.StopCondition == Triggers.Interacted);
                    inode.ControlNode = _ghostText.Submit(message: node.Message);
                }

                // Configure camrea is node action is camera.
                if (node.Action == Actions.Camera)
                {
                    Debug.Assert(node.StopCondition == Triggers.NotRunning);
                    _camera.Mode = node.CameraMode;
                    _camera.DuoObjectTracked = node.DuoObject;
                }

                // Prepare node for expression.
                if (node.Action == Actions.Express)
                {
                    Debug.Assert(
                        node.StopCondition == Triggers.Timeout ||
                        node.StopCondition == Triggers.Interacted);
                    inode.ControlNode = node.Character.SubmitExpress(node.ExpressID);
                }

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
                    var dialogueNode = (Dialogue.Node)inode.ControlNode;

                    // Close on timeout.
                    if (node.StopCondition == Triggers.Timeout && !dialogueNode.Closed && node.TimeOutFinished)
                        dialogueNode.Close();

                    // Stop on closed.
                    if (dialogueNode.Closed)
                        _stopNodes.Enqueue(node);
                }

                // Perform stop condition for ghost text.
                if (node.Action == Actions.GhostText)
                {
                    var ghostTextNode = (GhostText.Node)inode.ControlNode;

                    // Close on timeout.
                    if (node.StopCondition == Triggers.Timeout && !ghostTextNode.Closed && node.TimeOutFinished)
                        ghostTextNode.Close();

                    // Stop on closed.
                    if (ghostTextNode.Closed)
                        _stopNodes.Enqueue(node);
                }

                // Perform stop condition for camera action.
                if (node.Action == Actions.Camera)
                {
                    if (node.StopCondition == Triggers.NotRunning && !_camera.IsRunning)
                        _stopNodes.Enqueue(node);
                }

                if (node.Action == Actions.Express)
                {
                    var expressNode = (Character.ExpressNode)inode.ControlNode;

                    // close on timeout
                    if (node.StopCondition == Triggers.Timeout && !expressNode.Closed && node.TimeOutFinished)
                        expressNode.Close();

                    // Stop on closed.
                    if (expressNode.Closed)
                        _stopNodes.Enqueue(node);
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


/*
 * Action:Dialogue#
StartCondition:NoOutstanding#
StopCondition:Timeout#
Message:*CAW* *CAW* This is a test of my event and dialogue system!#
TimeOutValue:4.0;

Action:Camera#
StartCondition:Immediate#
StopCondition:NotRunning#
DuoObjectID:Raven#
CameraMode:CameraWalk;

Action:Dialogue#
StartCondition:NoOutstanding#
StopCondition:Timeout#
Message:Currently, dialogue messages can be queued up and the camera focus can be changed. Camera will focus on cat when dialogue is finished.#
TimeOutValue:4.0;

Action:GhostText#
StartCondition:Immediate#
StopCondition:Interacted#
Message:Press "Interact" to continue.;

Action:Dialogue#
StartCondition:NoOutstanding#
StopCondition:Timeout#
Message:I still need to add user interaction to continue the dialogue.#
TimeOutValue:4.0;

Action:Dialogue#
StartCondition:NoOutstanding#
StopCondition:Timeout#
Message:I'll be keeping the event system as simple as possible since the game isn't intended to be too sophisticated.#
TimeOutValue:4.0;

Action:Camera#
StartCondition:Immediate#
StopCondition:NotRunning#
DuoObjectID:Protag#
CameraMode:CameraWalk;

Action:Camera#
StartCondition:NoOutstanding#
StopCondition:NotRunning#
DuoObjectID:Protag#
CameraMode:BoxTrack
 */