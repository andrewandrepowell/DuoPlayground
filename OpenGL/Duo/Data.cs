using Duo.Managers;
using Microsoft.Xna.Framework.Graphics;
using Pow.Components;
using Pow.Utilities;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using nkast.Aether.Physics2D.Dynamics.Joints;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Input;
using Duo.Utilities;


namespace Duo.Data
{
    internal enum Maps { LevelDebug0, LevelDebug1 }
    internal enum Sprites { Cat, Pixel }
    internal enum Animations { CatWalk, CatIdle, Pixel }
    internal enum Boxes { Cat }
    public enum EntityTypes { DuoRunner, Camera, Surface, Cat, HUD, MainMenu, Dimmer }
    public class Data : IRunnerParent, IDuoRunnerParent
    {
        public Data() => DuoRunner.Initialize(this);
        public SizeF GameWindowSize => Globals.GameWindowSize;
        public float PixelsPerMeter => Globals.PixelsPerMeter;
        public string GumProjectFile => Globals.GumProjectFile;
        public void Initialize(Runner runner)
        {
            // maps
            runner.Map.Configure((int)Maps.LevelDebug0, "tiled/test_map_0");
            runner.Map.Configure((int)Maps.LevelDebug1, "tiled/test_map_1");
            // sprites / animations.
            runner.AnimationGenerator.ConfigureSprite(
                spriteId: (int)Sprites.Cat, 
                assetName: "images/cat_0", 
                regionSize: new(112, 112),
                directionSpriteEffects: new(new Dictionary<Directions, SpriteEffects>() 
                {
                    {Directions.Left, SpriteEffects.None},
                    {Directions.Right, SpriteEffects.FlipHorizontally},
                }));
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.CatWalk,
                spriteId: (int)Sprites.Cat, 
                spriteAnimationId: 0, 
                indices: [3, 4, 5, 6, 7, 8], 
                period: 0.25f, 
                repeat: true);
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.CatIdle,
                spriteId: (int)Sprites.Cat,
                spriteAnimationId: 1,
                indices: [1],
                period: 0.25f,
                repeat: false);
            runner.AnimationGenerator.ConfigureSprite(
                spriteId: (int)Sprites.Pixel,
                assetName: "images/pixel_0",
                regionSize: new(1, 1),
                directionSpriteEffects: new(new Dictionary<Directions, SpriteEffects>()
                {
                    {Directions.Left, SpriteEffects.None},
                    {Directions.Right, SpriteEffects.FlipHorizontally},
                }));
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.Pixel,
                spriteId: (int)Sprites.Pixel,
                spriteAnimationId: 0,
                indices: [0],
                period: 0,
                repeat: false);
            // entities
            runner.AddEntityType((int)EntityTypes.DuoRunner, world => world.Create(
                new StatusComponent(), 
                new GOCustomComponent<DuoRunner>()));
            runner.AddEntityType((int)EntityTypes.Camera, world => world.Create(
                new StatusComponent(),
                new GOCustomComponent<Managers.Camera>()));
            runner.AddEntityType((int)EntityTypes.Cat, world => world.Create(
                new StatusComponent(), 
                new AnimationComponent(),
                new PhysicsComponent(),
                new ControlComponent(),
                new GOCustomComponent<Cat>()));
            runner.AddEntityType((int)EntityTypes.Surface, world => world.Create(
                new StatusComponent(), 
                new PhysicsComponent(), 
                new GOCustomComponent<Surface>()));
            runner.AddEntityType((int)EntityTypes.HUD, world => world.Create(
                new StatusComponent(),
                new GumComponent(),
                new GOCustomComponent<HUD>()));
            runner.AddEntityType((int)EntityTypes.MainMenu, world => world.Create(
                new StatusComponent(),
                new GumComponent(),
                new ControlComponent(),
                new GOCustomComponent<MainMenu>()));
            runner.AddEntityType((int)EntityTypes.Dimmer, world => world.Create(
                new StatusComponent(),
                new AnimationComponent(),
                new GOCustomComponent<Dimmer>()));
            // custom GO managers.
            runner.AddGOCustomManager<DuoRunner>();
            runner.AddGOCustomManager<Managers.Camera>();
            runner.AddGOCustomManager<Cat>();
            runner.AddGOCustomManager<Surface>();
            runner.AddGOCustomManager<HUD>();
            runner.AddGOCustomManager<MainMenu>();
            runner.AddGOCustomManager<Dimmer>();
        }
        public void Initialize(DuoRunner duoRunner)
        {
            // Environment
            duoRunner.AddEnvironment<Managers.Camera>(EntityTypes.Camera);
            duoRunner.AddEnvironment<Cat>(EntityTypes.Cat);
            duoRunner.AddEnvironment<Surface>(EntityTypes.Surface);
            duoRunner.AddEnvironment<HUD>(EntityTypes.HUD);
            duoRunner.AddEnvironment<MainMenu>(EntityTypes.MainMenu);
            duoRunner.AddEnvironment<Dimmer>(EntityTypes.Dimmer);
            // Boxes
            duoRunner.BoxesGenerator.Configure(
                id: (int)Boxes.Cat, 
                assetName: "tiled/cat_boxes_0");
            // Controls
            duoRunner.UAGenerator.Configure((int)Controls.Menu);
            duoRunner.UAGenerator.Configure((int)Controls.Interact);
            duoRunner.UAGenerator.Configure((int)Controls.Jump);
            duoRunner.UAGenerator.Configure((int)Controls.MoveLeft);
            duoRunner.UAGenerator.Configure((int)Controls.MoveRight);
            // Controls - keyboard
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.Menu,
                key: Keys.Escape);
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.Jump,
                key: Keys.Space);
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.MoveLeft,
                key: Keys.Left);
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.MoveRight,
                key: Keys.Right);
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.Interact,
                key: Keys.Z);
            // Controls - Gamepad
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.Menu,
                button: Buttons.Start);
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.Jump,
                button: Buttons.A);
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.MoveLeft,
                button: Buttons.DPadLeft);
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.MoveRight,
                button: Buttons.DPadRight);
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.Interact,
                button: Buttons.X);
        }
        public void Initialize(Map.MapNode node)
        {
            var duoRunner = Globals.DuoRunner;
            foreach (ref var polygonNode in node.PolygonNodes.AsSpan())
                duoRunner.AddEnvironment(polygonNode);
        }
    }
}
