using System;
using GameLoop.Music;
using GameLoop.Persistence;
using GameLoop.UserSettings;
using Gameplay.Audio;
using Gameplay.Entities;
using Gameplay.Rendering.Effects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Content;

namespace GameLoop;

internal static class ServiceConfiguration
{
    internal static IServiceProvider ConfigureServices(ContentManager contentManager)
    {
        var services = new ServiceCollection();

        services
            .AddSingleton(contentManager)
            .AddSingleton<MusicPlayer>();

        ConfigureOptions(services);

        // Register gameplay services as transient (new instance per scene)
        services.AddTransient<EntityManager>();
        services.AddTransient<IAudioPlayer, AudioPlayer>();
        services.AddTransient<EffectManager>();

        return services.BuildServiceProvider();
    }

    private static void ConfigureOptions(ServiceCollection services)
    {
        var persistence = new AppDataPersistence();
        var configuration = new ConfigurationBuilder()
            .AddPersistence(persistence, "GameSettings")
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IPersistence>(persistence);

        services.Configure<AudioSettings>(configuration.GetSection("Audio"));
    }
}