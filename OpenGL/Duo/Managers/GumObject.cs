using Pow.Components;
using Pow.Utilities;
using Pow.Utilities.Gum;
using Arch.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal abstract class GumObject : Environment
    {
        private GumManager _gumManager;
        protected GumManager GumManager => _gumManager;
        public override void Initialize(PolygonNode node)
        {
            base.Initialize(node);
            _gumManager = Entity.Get<GumComponent>().Manager;
        }
    }
}
