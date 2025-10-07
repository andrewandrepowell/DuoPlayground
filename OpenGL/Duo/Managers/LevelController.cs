using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers;

internal class LevelController : Environment
{
    private bool _initialized;
    private string _uiID;
    private UI _ui;
    private string _branchesID;
    private TransitionBranches _branches;
    private string _mainID;
    private MainMenu _main;
    private States _state;
    public enum States { Waiting, OpeningBranches, OpeningUI, Running }
    public override void Initialize(PolygonNode node)
    {
        base.Initialize(node);
        {
            _initialized = false;
        }
        {
            _ui = null;
            _uiID = node.Parameters.GetValueOrDefault("UIID", "UI");
        }
        {
            _branches = null;
            _branchesID = node.Parameters.GetValueOrDefault("BranchesID", "Branches");
        }
        {
            _main = null;
            _mainID = node.Parameters.GetValueOrDefault("MainID", "Main");
        }
        {
            _state = States.Waiting;
        }
    }
    public override void Update()
    {
        base.Update();

        if (_ui == null)
        {
            Debug.Assert(!_initialized);
            var ui = Globals.DuoRunner.Environments.OfType<UI>().Where(ui => ui.ID == _uiID).First();
            if (ui.Initialized)
            {
                Debug.Assert(ui.Action == UI.Actions.Waiting);
                _ui = ui;
            }
        }

        if (_branches == null)
        {
            Debug.Assert(!_initialized);
            var branches  = Globals.DuoRunner.Environments.OfType<TransitionBranches>().Where(branches => branches.ID == _branchesID).First();
            if (branches.Initialized)
            {
                Debug.Assert(branches.State == RunningStates.Waiting);
                _branches = branches;
            }
        }

        if (_main == null)
        {
            Debug.Assert(!_initialized);
            var main = Globals.DuoRunner.Environments.OfType<MainMenu>().Where(main => main.ID == _mainID).First();
            if (main.Initialized)
            {
                Debug.Assert(main.State == RunningStates.Waiting);
                _main = main;
            }
        }

        if (!_initialized && _ui != null && _branches != null && _main != null)
        {
            _initialized = true;
            _branches.Open();
            _state = States.OpeningBranches;
        }

        if (_state == States.OpeningBranches && _branches.State == RunningStates.Running)
        {
            _ui.Open();
            _state = States.OpeningUI;
        }

        if (_state == States.OpeningUI && _ui.Action == UI.Actions.Idle)
        {
            _state = States.Running;
        }
    }
    public States State => _state;
}
