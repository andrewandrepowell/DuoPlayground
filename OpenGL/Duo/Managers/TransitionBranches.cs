using Duo.Data;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers;

internal class TransitionBranches : Environment
{
    private string _dimmerID;
    private Dimmer _dimmer;
    private string _transitionID;
    private Image _transition;
    private RunningStates _state;
    private bool _initialized;
    public override void Initialize(PolygonNode node)
    {
        base.Initialize(node);
        {
            _dimmer = null;
            _dimmerID = node.Parameters.GetValueOrDefault("DimmerID", "Dimmer");
        }
        {
            _transition = null;
            _transitionID = node.Parameters.GetValueOrDefault("TransitionID", "Transition");
        }
        {
            _state = RunningStates.Waiting;
            _initialized = false;
        }
    }
    public override void Update()
    {
        base.Update();

        if (_dimmer == null)
        {
            Debug.Assert(!_initialized);
            _dimmer = Globals.DuoRunner.Environments.OfType<Dimmer>().Where(dimmer => dimmer.ID == _dimmerID).First();
        }

        if (_transition == null)
        {
            Debug.Assert(!_initialized);
            _transition = Globals.DuoRunner.Environments.OfType<Image>().Where(i => i.ID == _transitionID).First();
        }

        if (!_initialized && _dimmer != null && _transition != null && !_transition.Running)
        {
            _initialized = true;
        }

        // Update state once dimmer component is finished its operation.
        if (_state == RunningStates.Starting && _dimmer.State == RunningStates.Waiting && !_transition.Running)
            ForceOpen();
        else if (_state == RunningStates.Stopping && _dimmer.State == RunningStates.Running && !_transition.Running)
            ForceClose();
    }
    public bool Initialized => _initialized;
    public RunningStates State
    {
        get
        {
            Debug.Assert(_initialized);
            return _state;
        }
    }
    public void Open()
    {
        Debug.Assert(_initialized);
        Debug.Assert(_state == RunningStates.Waiting);
        Debug.Assert(_dimmer.State == RunningStates.Running);
        Debug.Assert(!_transition.Running && _transition.Animation == Animations.TransitionRunning);
        _dimmer.Dimness = 1;
        _dimmer.Stop();
        _transition.Play(Animations.TransitionStopping);
        _transition.Visibility = 1;
        _state = RunningStates.Starting;
    }
    public void Close()
    {
        Debug.Assert(_initialized);
        Debug.Assert(_state == RunningStates.Running);
        Debug.Assert(_dimmer.State == RunningStates.Waiting);
        Debug.Assert(!_transition.Running && _transition.Animation == Animations.TransitionWaiting);
        _dimmer.Dimness = 1;
        _dimmer.Start();
        _transition.Play(Animations.TransitionStarting);
        _transition.Visibility = 1;
        _state = RunningStates.Stopping;
    }
    public void ForceOpen()
    {
        _dimmer.Dimness = 1;
        _dimmer.ForceStop();
        _transition.Play(Animations.TransitionWaiting);
        _transition.Visibility = 0;
        _state = RunningStates.Running;
    }
    public void ForceClose()
    {
        _dimmer.Dimness = 1;
        _dimmer.ForceStart();
        _transition.Play(Animations.TransitionRunning);
        _transition.Visibility = 1;
        _state = RunningStates.Waiting;
    }
}
