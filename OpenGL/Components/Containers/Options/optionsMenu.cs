using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;

using RenderingLibrary.Graphics;

using System.Linq;

namespace DuoGum.Components
{
    public interface OptionsButtons
    { 
        public bool IsFocused { get; set; }
        public string text_bText { get; }
        public InteractiveGue Visual { get; }
        public float containerRotation { get; }
    }
    partial class optionsMenu
    {
        private OptionsButtons[] _buttons;
        private void CheckFocus()
        {
#if DEBUG
            var total = 0;
            foreach (var button in _buttons)
                if (button.IsFocused)
                    total++;
            System.Diagnostics.Debug.Assert(total <= 1);
#endif
        }
        partial void CustomInitialize()
        {
            _buttons = [back, music, sfx, fw];
        }
        public OptionsButtons[] Buttons => _buttons;
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
