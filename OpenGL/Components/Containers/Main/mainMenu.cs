using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;

using RenderingLibrary.Graphics;
using System.Diagnostics;
using System.Linq;

namespace DuoGum.Components
{
    partial class mainMenu
    {
        private mainButton[] _buttons;
        private void CheckFocus()
        {
#if DEBUG
            var total = 0;
            foreach (var button in _buttons)
                if (button.IsFocused)
                    total++;
            Debug.Assert(total <= 1);
#endif
        }
        partial void CustomInitialize()
        {
            _buttons = [resume, options, exit];
        }
        public mainButton[] Buttons => _buttons;
        public void ResetFocus()
        {
            CheckFocus();
            foreach (var button in _buttons)
                button.IsFocused = false;
        }
        public bool ButtonFocused
        {
            get
            {
                CheckFocus();
                foreach (var button in _buttons)
                    if (button.IsFocused)
                        return true;
                return false;
            }
        }
    }
}
