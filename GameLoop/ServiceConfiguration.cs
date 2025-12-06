using System;
using Gameplay.Audio;
using Gameplay.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Content;

namespace GameLoop;

internal static class ServiceConfiguration
{
    internal static IServiceProvider ConfigureServices(ContentManager contentManager)
    {
        var services = new ServiceCollection();

        services.AddSingleton(contentManager);

        // Register gameplay services as transient (new instance per scene)
        services.AddTransient<EntityManager>();
        services.AddTransient<IAudioPlayer, AudioPlayer>();

        return services.BuildServiceProvider();
    }
}