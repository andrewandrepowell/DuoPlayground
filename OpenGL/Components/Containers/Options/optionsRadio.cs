using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;

using RenderingLibrary.Graphics;

using System.Linq;

namespace DuoGum.Components
{
    partial class optionsRadio : OptionsButtons
    {
        partial void CustomInitialize()
        {
            IsUsingLeftAndRightGamepadDirectionsForNavigation = false;
        }
    }
}
