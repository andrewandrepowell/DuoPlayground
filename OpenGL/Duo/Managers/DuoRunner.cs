using Pow.Utilities;
using Pow.Utilities.GO;
using System.Diagnostics;
using DuoGlobals = Duo.Globals;

namespace Duo.Managers
{
    public interface IDuoRunnerParent
    {
        public void Initialize(DuoRunner duoRunner);
    }
    public partial class DuoRunner : GOCustomManager
    {
        private static IDuoRunnerParent _parent;
        private static bool _parentInitialized = false;
        private bool _initialized = false;
        public static void Initialize(IDuoRunnerParent parent)
        {
            Debug.Assert(!_parentInitialized);
            _parent = parent;
            _parentInitialized = true;
        }
        public DuoRunner()
        {
            _environmentsEnumerable = new(_environments);
        }
        public override void Initialize() 
        {
            base.Initialize();
            Debug.Assert(_parentInitialized);
            Debug.Assert(!_initialized);
            _parent.Initialize(this);
            InitializeGenerators();
            _initialized = true;
            DuoGlobals.Initialize(this);
        }
        public override void Update()
        {

            Debug.Assert(_initialized);
            EnvironmentUpdate();
            base.Update();
        }
    }
}
