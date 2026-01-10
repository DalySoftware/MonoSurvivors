using System;
using Autofac;
using ContentLibrary;
using GameLoop.Input;
using GameLoop.Scenes.Gameplay.UI;
using Gameplay.Behaviour;
using Gameplay.Combat;
using Gameplay.Combat.Weapons;
using Gameplay.Combat.Weapons.AreaOfEffect;
using Gameplay.Combat.Weapons.OnHitEffects;
using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Entities;
using Gameplay.Entities.Effects;
using Gameplay.Entities.Enemies.Spawning;
using Gameplay.Entities.Pooling;
using Gameplay.Levelling;
using Gameplay.Levelling.PowerUps;
using Gameplay.Levelling.SphereGrid;
using Gameplay.Levelling.SphereGrid.Generation;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.Effects;
using Gameplay.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Gameplay;

internal class MainGameScene(
    SpriteBatch spriteBatch,
    EntityManager entityManager,
    EffectManager effectManager,
    EnemySpawner spawner,
    ChaseCamera camera,
    ContentManager content,
    ExperienceBar experienceBar,
    EntityRenderer entityRenderer,
    GameplayInputManager input,
    InputGate inputGate,
    HealthBar healthBar,
    RunClock clock,
    BossHealthBar bossHealthBar)
    : IScene
{
    private readonly static Color BackgroundColor = ColorPalette.Wine.ShiftChroma(-0.02f);
    private readonly Texture2D _backgroundTile = content.Load<Texture2D>(Paths.Images.BackgroundTile);

    public void Dispose() { }

    public void Update(GameTime gameTime)
    {
        if (inputGate.ShouldProcessInput())
            input.Update();
        effectManager.Update(gameTime);
        entityManager.Update(gameTime);
        camera.Update(gameTime);
        spawner.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        DrawBackground();

        entityRenderer.Draw(entityManager.Entities);
        healthBar.Draw(spriteBatch);
        experienceBar.Draw(spriteBatch, gameTime);
        clock.Draw(spriteBatch);
        bossHealthBar.Draw(spriteBatch);
    }

    private void DrawBackground()
    {
        spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: camera.Transform,
            sortMode: SpriteSortMode.FrontToBack);
        spriteBatch.Draw(_backgroundTile, camera.VisibleWorldBounds, camera.VisibleWorldBounds, BackgroundColor);
        spriteBatch.End();
    }

    internal static void ConfigureServices(ContainerBuilder builder)
    {
        builder.RegisterType<BulletSplitOnHit>();
        builder.RegisterType<ChainLightningOnHit>();

        builder.RegisterType<BasicGun>();
        builder.RegisterType<Shotgun>();
        builder.RegisterType<SniperRifle>();
        builder.RegisterType<IceAura>();
        builder.RegisterType<BouncingGun>();

        builder.RegisterType<IceAuraEffect>().OnActivated(a => a.Context.Resolve<EntityManager>().Spawn(a.Instance));

        builder.RegisterType<WeaponFactory>();
        builder.RegisterType<WeaponBelt>().SingleInstance();

        builder.RegisterInstance(typeof(BasicGun))
            .As<Type>()
            .Keyed<Type>("StartingWeapon");


        builder.RegisterType<BulletPool>().SingleInstance();
        builder.RegisterType<EnemyDeathBlast>().SingleInstance();
        builder.RegisterType<HealthRegenManager>().SingleInstance();

        builder.RegisterType<PlayerStats>().SingleInstance();
        builder.RegisterType<PlayerCharacter>().SingleInstance()
            .WithParameter((pi, _) => pi.Name == "position", (_, _) => new Vector2(0, 0))
            .OnActivated(a =>
            {
                var startingWeapon = a.Context.ResolveKeyed<WeaponDescriptor>("StartingWeapon");
                a.Instance.AddPowerUp(startingWeapon.Unlock);
                a.Context.Resolve<ISpawnEntity>().Spawn(a.Instance);
            });


        builder.RegisterType<EffectManager>().SingleInstance();
        builder.RegisterType<EntityManager>().AsSelf().As<ISpawnEntity>().As<IEntityFinder>().SingleInstance();
        builder.RegisterType<ExperienceSpawner>().SingleInstance();
        builder.RegisterType<ScreenPositioner>().WithParameter(new NamedParameter("buffer", 0.3f));
        builder.RegisterType<EnemyFactory>();
        builder.RegisterType<EnemySpawner>().SingleInstance();
        builder.RegisterType<CritCalculator>().SingleInstance();

        builder.RegisterInstance(new LevelCalculator(4, 1.4f));
        builder.RegisterType<LevelManager>().SingleInstance();

        builder.RegisterType<EntityRenderer>().SingleInstance();
        builder.RegisterType<OutlineRenderer>().SingleInstance();

        builder.RegisterType<ChaseCamera>()
            .WithParameter((pi, _) => pi.Name == "target", (_, ctx) => ctx.Resolve<PlayerCharacter>())
            .SingleInstance();

        builder.Register<SphereGrid>(ctx =>
        {
            var player = ctx.Resolve<PlayerCharacter>();
            var startingWeapon = ctx.ResolveKeyed<WeaponDescriptor>("StartingWeapon");
            return GridFactory.CreateRandom(player.AddPowerUp, startingWeapon);
        }).InstancePerLifetimeScope();

        builder.RegisterType<ExperienceBarFactory>().SingleInstance();
        builder.Register<ExperienceBar>(ctx => ctx.Resolve<ExperienceBarFactory>().Create());

        builder.RegisterType<BossHealthBarFactory>().SingleInstance();
        builder.Register<BossHealthBar>(ctx => ctx.Resolve<BossHealthBarFactory>().Create());

        builder.RegisterType<HealthBarFactory>().SingleInstance();
        builder.Register<HealthBar>(ctx => ctx.Resolve<HealthBarFactory>().Create());

        builder.RegisterType<RunClockFactory>().SingleInstance();
        builder.Register<RunClock>(ctx => ctx.Resolve<RunClockFactory>().Create());

        builder.RegisterType<GameplayInputManager>()
            .SingleInstance();

        builder.RegisterType<MainGameScene>().InstancePerDependency();
    }
}