using Autofac;
using GameLoop.Audio;
using GameLoop.Rendering;
using GameLoop.Scenes;
using Gameplay;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.DependencyInjection;

public sealed class GameContainer
{
    public GameContainer(CoreGame game, GraphicsDeviceManager graphicsManager)
    {
        var builder = new ContainerBuilder();

        builder.RegisterInstance(game)
            .As<Game>()
            .As<IGlobalCommands>()
            .SingleInstance();

        builder.RegisterInstance(game.Window).SingleInstance();

        builder.RegisterInstance(graphicsManager).ExternallyOwned();
        builder.Register(ctx => ctx.Resolve<GraphicsDeviceManager>().GraphicsDevice)
            .SingleInstance().ExternallyOwned();
        builder.RegisterType<SpriteBatch>().SingleInstance();
        builder.RegisterType<RenderScaler>().AsSelf().As<IRenderViewport>().SingleInstance();
        builder.RegisterType<DisplayModeManager>().SingleInstance();

        builder.RegisterType<SceneManager>().SingleInstance();

        builder.RegisterType<MusicDucker>().SingleInstance();

        builder.ConfigureOptions();

        Root = builder.Build();
    }

    public IContainer Root { get; }
}