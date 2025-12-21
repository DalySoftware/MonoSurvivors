using Autofac;
using GameLoop.Scenes;
using GameLoop.Scenes.Title;
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

        builder.RegisterType<SceneManager>()
            .WithParameter((pi, _) => pi.Name == "initial", (_, ctx) => ctx.Resolve<TitleScene>())
            .SingleInstance();

        builder.ConfigureOptions();

        Root = builder.Build();
    }

    public IContainer Root { get; }
}