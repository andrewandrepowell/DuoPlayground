using Pow.Utilities;
using Pow.Utilities.UA;
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
        private readonly UAGenerator _uaGenerator = new();
        public BoxesGenerator BoxesGenerator => _boxesGenerator;
        internal UAGenerator UAGenerator => _uaGenerator;
        private void InitializeGenerators()
        {
            _boxesGenerator.Initialize();
            _uaGenerator.Initalize();
        }
    }
}
