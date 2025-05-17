using Duo.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Managers
{
    internal partial class Character
    {
        protected abstract IReadOnlyDictionary<Actions, Animations> ActionAnimationMap { get; }
        protected abstract string BoxesAssetName { get; }
    }
}
