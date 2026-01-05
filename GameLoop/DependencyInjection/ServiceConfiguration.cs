using Autofac;
using GameLoop.Persistence;
using GameLoop.UserSettings;

namespace GameLoop.DependencyInjection;

internal static class ServiceConfiguration
{
    extension(ContainerBuilder builder)
    {
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