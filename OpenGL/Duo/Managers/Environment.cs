using Pow.Utilities.GO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pow.Utilities;
using System.Diagnostics;

namespace Duo.Managers
{
    public abstract class Environment : GOCustomManager
    {
        public virtual void Initialize(Map.PolygonNode node)
        {
        }
    }
}
