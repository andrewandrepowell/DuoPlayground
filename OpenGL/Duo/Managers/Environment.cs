using Pow.Utilities.GO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pow.Utilities;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Duo.Managers
{
    public abstract class Environment : GOCustomManager
    {
        private bool _initialized = false;
        private string _id = null;
        public string ID => _id;
        public virtual void Initialize(Map.PolygonNode node)
        {
            Debug.Assert(!_initialized);
            _id = node.Parameters.GetValueOrDefault("ID", null);
            _initialized = true;
        }
        public override void Cleanup()
        {
            Debug.Assert(_initialized);
            _initialized = false;
            base.Cleanup();
        }
    }
}
