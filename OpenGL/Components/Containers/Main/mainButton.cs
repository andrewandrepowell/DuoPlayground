using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;
using MonoGameGum;
using MonoGameGum.Forms.Controls;
using RenderingLibrary.Graphics;
using System;
using System.Linq;

namespace DuoGum.Components
{
    partial class mainButton
    {
        partial void CustomInitialize()
        {
            Click += (_, _) => IsFocused = true;
            KeyDown += HandleKeyDown;
        }
        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine($"Debug: {e.Key}. Button: {Message}. Category: {ButtonCategoryState}");
            switch (e.Key)
            {
                case Microsoft.Xna.Framework.Input.Keys.Up:
                    HandleTab(TabDirection.Up, loop: true);
                    break;
                case Microsoft.Xna.Framework.Input.Keys.Down:
                    HandleTab(TabDirection.Down, loop: true);
                    break;
                case Microsoft.Xna.Framework.Input.Keys.Enter:
                    PerformClick(GumService.Default.Keyboard);
                    break;
            }
        }
    }
}
