using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;

using RenderingLibrary.Graphics;
using System;
using System.Linq;

namespace DuoGum.Components
{
    partial class titleButtonContainer
    {
        private static readonly Random _random = new();
        private const float _rotationMagnitude = 8.5f;
        private const float _rotationOffset = -_rotationMagnitude / 2.0f;
        partial void CustomInitialize()
        {
            Visual.Rotation = _random.NextSingle() * _rotationMagnitude + _rotationOffset;
        }
    }
}
