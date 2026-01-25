using Autofac;
using GameLoop.Persistence;
using GameLoop.Rendering;
using Gameplay;
using Gameplay.Audio;
using Veil.Desktop.PlatformServices.Audio;
using Veil.Desktop.PlatformServices.Lifecycle;
using Veil.Desktop.PlatformServices.Persistence;
using Veil.Desktop.PlatformServices.Rendering;

namespace Veil.Desktop.PlatformServices;

internal static class ServiceConfigurator
{
    public static void Configure(ContainerBuilder builder)
    {
        builder.RegisterType<MusicPlayer>().As<IMusicPlayer>().SingleInstance();
        builder.RegisterType<SoundEffectPlayer>().As<IAudioPlayer>().SingleInstance();

        builder.RegisterType<AppDataPersistence>().As<ISettingsPersistence>().SingleInstance();

        builder.RegisterType<DisplayModeManager>().As<IDisplayModeManager>().SingleInstance();
        builder.RegisterType<DesktopViewportSync>().As<IViewportSync>().SingleInstance();
        builder.RegisterType<DesktopBackground>().As<IBackground>().SingleInstance();

        builder.RegisterType<DesktopAppLifeCycle>().As<IAppLifeCycle>().SingleInstance();
    }
}