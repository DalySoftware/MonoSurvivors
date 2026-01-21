using Autofac;
using GameLoop.Persistence;
using Gameplay.Audio;
using Veil.Desktop.PlatformServices.Audio;
using Veil.Desktop.PlatformServices.Persistence;

namespace Veil.Desktop.PlatformServices;

internal static class ServiceConfigurator
{
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterType<MusicPlayer>().As<IMusicPlayer>().SingleInstance();
        builder.RegisterType<SoundEffectPlayer>().As<IAudioPlayer>().SingleInstance();

        builder.RegisterType<AppDataPersistence>().As<ISettingsPersistence>().SingleInstance();
    }
}