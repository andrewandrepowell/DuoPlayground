using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duo.Utilities
{
    internal static partial class ParticleEffects
    {
        internal static ParticleEffect CreateMenuWindParticleEffect()
        {
            var textureRegions = CreateTexture2DRegions(
                texture: Pow.Globals.Game.Content.Load<Texture2D>("images/particles_0"),
                regionSize: new Size(80, 80));
            var emitters = Enumerable.Range(0, 2).Select(x => new ParticleEmitter(
                textureRegion: textureRegions[x],
                capacity: 100,
                lifeSpan: TimeSpan.FromSeconds(5.0f),
                profile: new ExtendedLineProfile()
                {
                    Axis = new(x: 0, y: 1),
                    Direction = new(x: 1, y: +0.1f),
                    Cone = MathHelper.Pi * 0.1f,
                    Length = Globals.GameWindowSize.Height,
                })
            {
                Parameters = new ParticleReleaseParameters
                {
                    Quantity = 1,
                    Speed = new Range<float>(256 + 32, 256 + 64),
                    Scale = 1,
                    Rotation = 0
                },
                Modifiers =
                        {
                            new LinearGravityModifier { Direction = Vector2.UnitY, Strength = 40.0f },
                            new RotationModifier() { RotationRate = 0.25f },
                        },
                AutoTriggerFrequency = 1,
                AutoTrigger = true,
            }).ToList();
            var particleEffect = new ParticleEffect()
            {
                Emitters = emitters
            };
            return particleEffect;
        }
    }
}
