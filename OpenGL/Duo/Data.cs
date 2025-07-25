﻿using Duo.Managers;
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
    internal enum Controls { MoveLeft, MoveRight, Jump, Interact, Menu }
    internal enum Maps { LevelDebug0, LevelDebug1, LevelDebug2 }
    internal enum Sprites 
    { 
        Cat, Pixel, Platform, Background, PurpleHillsBackground,
        TreeRoot
    }
    internal enum Animations 
    { 
        CatWalk, CatIdle, CatJump, CatFall, CatLand, 
        Pixel, 
        Platform, 
        Background, PurpleHillsSkyBox, 
        PurpleHillsFarClouds, PurpleHillsMidClouds, 
        PurpleHillsFarMountains, PurpleHillsMidMountains, PurpleHillsCloseMountains,
        PurpleHillsFarTrees, PurpleHillsMidTrees,
        TreeRootIdle, TreeRootDeath, TreeRootTwitch
    }
    internal enum Boxes { Cat, Root }
    public enum EntityTypes { DuoRunner, Camera, Surface, Cat, Key, HUD, MainMenu, Dimmer, Background }
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
            runner.Map.Configure((int)Maps.LevelDebug2, "tiled/test_map_2");
            // sprites / animations.
            runner.AnimationGenerator.ConfigureSprite(
                spriteId: (int)Sprites.Cat, 
                assetName: "images/cat_0", 
                regionSize: new(144, 144),
                directionSpriteEffects: new(new Dictionary<Directions, SpriteEffects>() 
                {
                    {Directions.Left, SpriteEffects.None},
                    {Directions.Right, SpriteEffects.FlipHorizontally},
                }));
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.CatWalk,
                spriteId: (int)Sprites.Cat, 
                spriteAnimationId: 0, 
                indices: [8, 9, 10, 11, 12, 13], 
                period: 0.15f, 
                repeat: true);
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.CatJump,
                spriteId: (int)Sprites.Cat,
                spriteAnimationId: 1,
                indices: [14, 15, 16, 17],
                period: 0.15f,
                repeat: false);
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.CatFall,
                spriteId: (int)Sprites.Cat,
                spriteAnimationId: 4,
                indices: [18],
                period: 0.25f,
                repeat: false);
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.CatLand,
                spriteId: (int)Sprites.Cat,
                spriteAnimationId: 5,
                indices: [19, 20],
                period: 0.15f,
                repeat: false);
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.CatIdle,
                spriteId: (int)Sprites.Cat,
                spriteAnimationId: 6,
                indices: [21, 22, 23, 24, 25, 26, 27, 28, 29, 30],
                period: 0.7f,
                repeat: true);
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
            runner.AnimationGenerator.ConfigureSprite(
                spriteId: (int)Sprites.Platform,
                assetName: "images/platform_1",
                regionSize: new(224, 64),
                directionSpriteEffects: new(new Dictionary<Directions, SpriteEffects>()
                {
                    {Directions.Left, SpriteEffects.None},
                    {Directions.Right, SpriteEffects.FlipHorizontally},
                }));
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.Platform,
                spriteId: (int)Sprites.Platform,
                spriteAnimationId: 0,
                indices: [0],
                period: 0,
                repeat: false);
            runner.AnimationGenerator.ConfigureSprite(
                spriteId: (int)Sprites.Background,
                assetName: "images/background_0",
                regionSize: (Size)Globals.GameWindowSize,
                directionSpriteEffects: new(new Dictionary<Directions, SpriteEffects>()
                {
                    {Directions.Left, SpriteEffects.None},
                    {Directions.Right, SpriteEffects.FlipHorizontally},
                }));
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.Background,
                spriteId: (int)Sprites.Background,
                spriteAnimationId: 0,
                indices: [0],
                period: 0,
                repeat: false);
            runner.AnimationGenerator.ConfigureSprite(
                spriteId: (int)Sprites.PurpleHillsBackground,
                assetName: "images/background_1",
                regionSize: (Size)Globals.GameWindowSize,
                directionSpriteEffects: new(new Dictionary<Directions, SpriteEffects>()
                {
                    {Directions.Left, SpriteEffects.None},
                    {Directions.Right, SpriteEffects.FlipHorizontally},
                }));
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.PurpleHillsSkyBox,
                spriteId: (int)Sprites.PurpleHillsBackground,
                spriteAnimationId: 0,
                indices: [0],
                period: 0,
                repeat: false);
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.PurpleHillsFarClouds,
                spriteId: (int)Sprites.PurpleHillsBackground,
                spriteAnimationId: 1,
                indices: [1],
                period: 0,
                repeat: false);
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.PurpleHillsMidClouds,
                spriteId: (int)Sprites.PurpleHillsBackground,
                spriteAnimationId: 2,
                indices: [2],
                period: 0,
                repeat: false);
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.PurpleHillsFarMountains,
                spriteId: (int)Sprites.PurpleHillsBackground,
                spriteAnimationId: 3,
                indices: [3],
                period: 0,
                repeat: false);
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.PurpleHillsMidMountains,
                spriteId: (int)Sprites.PurpleHillsBackground,
                spriteAnimationId: 4,
                indices: [4],
                period: 0,
                repeat: false);
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.PurpleHillsCloseMountains,
                spriteId: (int)Sprites.PurpleHillsBackground,
                spriteAnimationId: 5,
                indices: [5],
                period: 0,
                repeat: false);
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.PurpleHillsFarTrees,
                spriteId: (int)Sprites.PurpleHillsBackground,
                spriteAnimationId: 6,
                indices: [6],
                period: 0,
                repeat: false);
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.PurpleHillsMidTrees,
                spriteId: (int)Sprites.PurpleHillsBackground,
                spriteAnimationId: 7,
                indices: [7],
                period: 0,
                repeat: false);
            runner.AnimationGenerator.ConfigureSprite(
                spriteId: (int)Sprites.TreeRoot,
                assetName: "images/root_0",
                regionSize: new(192, 160),
                directionSpriteEffects: new(new Dictionary<Directions, SpriteEffects>()
                {
                    {Directions.Left, SpriteEffects.None},
                    {Directions.Right, SpriteEffects.FlipHorizontally},
                }));
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.TreeRootIdle,
                spriteId: (int)Sprites.TreeRoot,
                spriteAnimationId: 0,
                indices: [0],
                period: 0,
                repeat: false);
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.TreeRootDeath,
                spriteId: (int)Sprites.TreeRoot,
                spriteAnimationId: 1,
                indices: Enumerable.Range(0, 15).ToArray(),
                period: 0.1f,
                repeat: false);
            runner.AnimationGenerator.ConfigureAnimation(
                animationId: (int)Animations.TreeRootTwitch,
                spriteId: (int)Sprites.TreeRoot,
                spriteAnimationId: 2,
                indices: Enumerable.Range(15, 12).ToArray(),
                period: 0.1f,
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
                new AnimationComponent(),
                new PhysicsComponent(), 
                new GOCustomComponent<Surface>()));
            runner.AddEntityType((int)EntityTypes.Key, world => world.Create(
                new StatusComponent(),
                new AnimationComponent(),
                new PhysicsComponent(),
                new GOCustomComponent<Key>()));
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
            runner.AddEntityType((int)EntityTypes.Background, world => world.Create(
                new StatusComponent(),
                new AnimationComponent(),
                new GOCustomComponent<Background>()));
            // custom GO managers.
            runner.AddGOCustomManager<DuoRunner>();
            runner.AddGOCustomManager<Managers.Camera>();
            runner.AddGOCustomManager<Cat>();
            runner.AddGOCustomManager<Surface>();
            runner.AddGOCustomManager<Key>();
            runner.AddGOCustomManager<HUD>();
            runner.AddGOCustomManager<MainMenu>();
            runner.AddGOCustomManager<Dimmer>();
            runner.AddGOCustomManager<Background>();
        }
        public void Initialize(DuoRunner duoRunner)
        {
            // Environment
            duoRunner.AddEnvironment<Managers.Camera>(EntityTypes.Camera);
            duoRunner.AddEnvironment<Cat>(EntityTypes.Cat);
            duoRunner.AddEnvironment<Surface>(EntityTypes.Surface);
            duoRunner.AddEnvironment<Key>(EntityTypes.Key);
            duoRunner.AddEnvironment<HUD>(EntityTypes.HUD);
            duoRunner.AddEnvironment<MainMenu>(EntityTypes.MainMenu);
            duoRunner.AddEnvironment<Dimmer>(EntityTypes.Dimmer);
            duoRunner.AddEnvironment<Background>(EntityTypes.Background);
            // Boxes
            duoRunner.BoxesGenerator.Configure(
                id: (int)Boxes.Cat, 
                assetName: "tiled/cat_boxes_0");
            duoRunner.BoxesGenerator.Configure(
                id: (int)Boxes.Root,
                assetName: "tiled/root_boxes_0");
            // Controls - Actions
            duoRunner.UAGenerator.Configure((int)Controls.Menu);
            duoRunner.UAGenerator.Configure((int)Controls.Interact);
            duoRunner.UAGenerator.Configure((int)Controls.Jump);
            duoRunner.UAGenerator.Configure((int)Controls.MoveLeft);
            duoRunner.UAGenerator.Configure((int)Controls.MoveRight);
            // Controls - Bindings
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.Menu,
                key: Keys.Escape);
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.Menu,
                button: Buttons.Start);
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.Jump,
                key: Keys.Space);
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.Jump,
                button: Buttons.A);
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.MoveLeft,
                key: Keys.Left);
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.MoveLeft,
                button: Buttons.DPadLeft);
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.MoveLeft,
                thumbstick: Directions.Left,
                target: new(x: -1, y: 0));
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.MoveRight,
                key: Keys.Right);
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.MoveRight,
                button: Buttons.DPadRight);
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.MoveRight,
                thumbstick: Directions.Left,
                target: new(x: 1, y: 0));
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.Interact,
                key: Keys.Z);
            duoRunner.UAGenerator.Configure(
                actionId: (int)Controls.Interact,
                button: Buttons.Y);
        }
        public void Initialize(Map.MapNode node)
        {
            var duoRunner = Globals.DuoRunner;
            foreach (ref var polygonNode in node.PolygonNodes.AsSpan())
                duoRunner.AddEnvironment(polygonNode);
        }
    }
}
