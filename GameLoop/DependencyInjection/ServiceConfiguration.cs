using Autofac;
using GameLoop.Audio;
using GameLoop.Persistence;
using GameLoop.UserSettings;
using Gameplay.Audio;
using Gameplay.Entities;
using Gameplay.Rendering;
using Gameplay.Rendering.Effects;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.DependencyInjection;

internal static class ServiceConfiguration
{
    extension(ContainerBuilder builder)
    {
        internal void ConfigureContentServices(GraphicsDevice graphicsDevice,
            ContentManager content)
        {
            builder.RegisterInstance(graphicsDevice)
                .SingleInstance();

            builder.RegisterInstance(content)
                .SingleInstance();

            builder.RegisterType<PrimitiveRenderer>()
                .SingleInstance();

            builder.RegisterType<MusicPlayer>()
                .SingleInstance();

            builder.RegisterType<SoundEffectPlayer>().As<IAudioPlayer>();

            builder.RegisterType<EffectManager>()
                .SingleInstance();

            builder.RegisterType<EntityManager>()
                .SingleInstance();
        }

        internal void ConfigureOptions()
        {
            builder.RegisterType<AppDataPersistence>()
                .As<ISettingsPersistence>()
                .SingleInstance();

            builder.RegisterDecorator<MergingSettingsPersistence, ISettingsPersistence>();

            builder.Register(ctx =>
                ctx.Resolve<ISettingsPersistence>()
                    .Load(PersistenceJsonContext.Default.AudioSettings));

            builder.Register(ctx =>
                ctx.Resolve<ISettingsPersistence>()
                    .Load(PersistenceJsonContext.Default.KeyBindingsSettings));
        }
    }
}