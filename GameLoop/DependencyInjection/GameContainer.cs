using Autofac;
using GameLoop.Scenes;
using Gameplay;
using Microsoft.Xna.Framework;

namespace GameLoop.DependencyInjection;

public sealed class GameContainer
{
    public GameContainer(CoreGame game)
    {
        var builder = new ContainerBuilder();

        builder.RegisterInstance(game)
            .As<Game>()
            .As<IGlobalCommands>()
            .SingleInstance();

        builder.RegisterInstance(game.Window)
            .SingleInstance();

        builder.RegisterType<SceneManager>().SingleInstance();

        builder.ConfigureOptions();

        Root = builder.Build();
    }

    public IContainer Root { get; }
}