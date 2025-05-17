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
        public BoxesGenerator BoxesGenerator => _boxesGenerator;
        private void InitializeGenerators()
        {
            _boxesGenerator.Initialize();
        }
    }
}
