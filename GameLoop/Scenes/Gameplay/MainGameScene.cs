using Autofac;
using ContentLibrary;
using GameLoop.Scenes.Gameplay.UI;
using Gameplay;
using Gameplay.Behaviour;
using Gameplay.Combat;
using Gameplay.Combat.Weapons;
using Gameplay.Combat.Weapons.AreaOfEffect;
using Gameplay.Combat.Weapons.OnHitEffects;
using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Entities;
using Gameplay.Entities.Effects;
using Gameplay.Entities.Enemies;
using Gameplay.Levelling;
using Gameplay.Levelling.SphereGrid;
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
    HealthBar healthBar,
    PlayTime playTime)
    : IScene
{
    private readonly Texture2D _backgroundTile = content.Load<Texture2D>(Paths.Images.BackgroundTile);

    public void Dispose() { }

    public void Update(GameTime gameTime)
    {
        input.Update(gameTime);
        effectManager.Update(gameTime);
        entityManager.Update(gameTime);
        camera.Update(gameTime);
        spawner.Update(gameTime);
        playTime.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        DrawBackground();

        entityRenderer.Draw(entityManager.Entities);
        healthBar.Draw(spriteBatch);
        experienceBar.Draw(spriteBatch, gameTime, Color.CadetBlue, Color.GreenYellow);
    }

    private void DrawBackground()
    {
        spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: camera.Transform,
            sortMode: SpriteSortMode.FrontToBack);
        spriteBatch.Draw(_backgroundTile, camera.VisibleWorldBounds, camera.VisibleWorldBounds,
            Color.DarkSlateGray.ShiftChroma(-0.02f).ShiftLightness(0.05f));
        spriteBatch.End();
    }

    internal static void ConfigureServices(ContainerBuilder builder)
    {
        builder.Register(ctx => ctx.Resolve<GraphicsDevice>().Viewport);

        builder.RegisterType<BulletSplitOnHit>();
        builder.RegisterType<ChainLightningOnHit>();


        builder.RegisterType<BasicGun>();
        builder.RegisterType<Shotgun>();
        builder.RegisterType<SniperRifle>();
        builder.RegisterType<DamageAura>();

        builder.RegisterType<DamageAuraEffect>().OnActivated(a => a.Context.Resolve<EntityManager>().Spawn(a.Instance));

        builder.RegisterType<WeaponFactory>();
        builder.RegisterType<WeaponBelt>()
            .OnActivated(a => a.Instance.AddWeapon(a.Context.Resolve<BasicGun>()))
            .SingleInstance();

        builder.RegisterType<HealthRegenManager>().SingleInstance();

        builder.RegisterType<PlayerStats>().SingleInstance();
        builder.RegisterType<PlayerCharacter>().SingleInstance()
            .WithParameter((pi, _) => pi.Name == "position", (_, _) => new Vector2(0, 0))
            .OnActivated(a => a.Context.Resolve<ISpawnEntity>().Spawn(a.Instance));


        builder.RegisterType<EffectManager>().SingleInstance();
        builder.RegisterType<EntityManager>().AsSelf().As<ISpawnEntity>().As<IEntityFinder>().SingleInstance();
        builder.RegisterType<ExperienceSpawner>().SingleInstance();
        builder.RegisterType<ScreenPositioner>().WithParameter(new NamedParameter("buffer", 0.3f));
        builder.RegisterType<EnemyFactory>();
        builder.RegisterType<EnemySpawner>().SingleInstance();

        builder.RegisterInstance(new LevelCalculator(5, 1.25f));
        builder.RegisterType<LevelManager>().SingleInstance();

        builder.RegisterType<EntityRenderer>().SingleInstance();

        builder.RegisterType<ChaseCamera>()
            .WithParameter((pi, _) => pi.Name == "target", (_, ctx) => ctx.Resolve<PlayerCharacter>())
            .SingleInstance();

        builder.Register<SphereGrid>(ctx =>
        {
            var player = ctx.Resolve<PlayerCharacter>();
            return GridFactory.CreateRandom(player.AddPowerUp);
        }).SingleInstance();

        builder.RegisterType<ExperienceBarFactory>().SingleInstance();
        builder.Register<ExperienceBar>(ctx => ctx.Resolve<ExperienceBarFactory>().Create());

        builder.RegisterType<HealthBar>()
            .WithProperty(h => h.Position, new Vector2(10, 10));

        builder.RegisterType<PlayTime>().AsSelf().As<IPlayTime>().SingleInstance();
        builder.RegisterType<GameplayInputManager>()
            .SingleInstance();

        builder.RegisterType<MainGameScene>().InstancePerDependency();
    }
}