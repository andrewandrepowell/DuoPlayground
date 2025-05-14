using Duo.Data;
using Pow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal abstract partial class Character : DuoObject
    {
        public override void Initialize()
        {
            base.Initialize();
        }
        public override void Initialize(Map.PolygonNode node)
        {
            base.Initialize(node);
            UpdateAction(Actions.Idle);
            InitializeDirection();
            InitializeMovement(node);
        }
        public override void Update()
        {
            MovementUpdate();
            base.Update();
        }

    }
}
