using Autofac;
using GameLoop.Persistence;
using Gameplay.Audio;
using Microsoft.JSInterop;
using Veil.Web.PlatformServices.Audio;
using Veil.Web.PlatformServices.Persistence;

namespace Veil.Web.PlatformServices;

internal static class ServiceConfigurator
{
    public static void Configure(ContainerBuilder builder, IJSRuntime jsRuntime)
    {
        builder.RegisterInstance(jsRuntime).As<IJSInProcessRuntime>().SingleInstance();
        builder.RegisterType<LocalStoragePersistence>().As<ISettingsPersistence>().SingleInstance();

        builder.RegisterType<WebMusicPlayer>().As<IMusicPlayer>().SingleInstance();
        builder.RegisterType<WebSoundEffectPlayer>().As<IAudioPlayer>().SingleInstance();

        builder.Register(ctx =>
            ctx.Resolve<ISettingsPersistence>()
                .Load(PersistenceJsonContext.Default.AudioSettings));

        builder.Register(ctx =>
            ctx.Resolve<ISettingsPersistence>()
                .Load(PersistenceJsonContext.Default.KeyBindingsSettings));
    }
}