using Duo.Utilities.Control;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    public partial class DuoRunner
    {
        private readonly BoxesGenerator _boxesGenerator = new();
        private readonly ControlGenerator _controlGenerator = new();
        public BoxesGenerator BoxesGenerator => _boxesGenerator;
        internal ControlGenerator ControlGenerator => _controlGenerator;
        private void InitializeGenerators()
        {
            _boxesGenerator.Initialize();
            _controlGenerator.Initalize();
        }
    }
}
