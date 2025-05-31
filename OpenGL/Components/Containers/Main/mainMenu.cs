using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;

using RenderingLibrary.Graphics;

using System.Linq;

namespace DuoGum.Components
{
    partial class mainMenu
    {
        private mainButton[] _buttons; 
        partial void CustomInitialize()
        {
            _buttons = [resume, options, exit];
        }
        public mainButton[] Buttons => _buttons;
        public void ResetFocus()
        {
            foreach (var button in _buttons)
                button.IsFocused = false;
        }
    }
}
