using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using GameLoop.Audio;
using GameLoop.Persistence;
using GameLoop.UserSettings;
using Gameplay.Audio;
using Gameplay.Entities;
using Gameplay.Rendering;
using Gameplay.Rendering.Effects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            var persistence = new AppDataPersistence();
            var configuration = new ConfigurationBuilder()
                .AddPersistence(persistence, "GameSettings")
                .Build();

            var serviceCollection = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddOptions(); // adds IOptions/IOptionsMonitor support

            serviceCollection.Configure<AudioSettings>(configuration.GetSection("Audio"));

            // Populate the ServiceCollection into Autofac
            builder.Populate(serviceCollection);
        }
    }
}

internal class OptionsMonitorWrapper<T>(IOptions<T> options) : IOptionsMonitor<T>
    where T : class
{
    public T CurrentValue { get; } = options.Value;

    public T Get(string? name) => CurrentValue;

    public IDisposable? OnChange(Action<T, string> listener) => new DummyDisposable();

    private class DummyDisposable : IDisposable
    {
        public void Dispose() { }
    }
}