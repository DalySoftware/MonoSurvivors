using Autofac;
using GameLoop.Rendering;
using GameLoop.Scenes;
using Gameplay;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        builder.RegisterInstance(game.Window).SingleInstance();

        builder.RegisterType<GraphicsDeviceManager>().SingleInstance();
        builder.Register(ctx => ctx.Resolve<GraphicsDeviceManager>().GraphicsDevice)
            .SingleInstance();
        builder.RegisterType<SpriteBatch>().SingleInstance();
        builder.RegisterType<RenderScaler>().AsSelf().As<IRenderViewport>().SingleInstance();
        builder.RegisterType<DisplayModeManager>().SingleInstance();

        builder.RegisterType<SceneManager>().SingleInstance();

        builder.ConfigureOptions();

        Root = builder.Build();

        _ = Root.Resolve<GraphicsDeviceManager>();
    }

    public IContainer Root { get; }
}