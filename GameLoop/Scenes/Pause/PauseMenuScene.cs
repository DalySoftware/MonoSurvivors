using Autofac;
using GameLoop.Scenes.Pause.UI;
using Microsoft.Xna.Framework;

namespace GameLoop.Scenes.Pause;

internal class PauseMenuScene(PauseInputManager input, PauseUi ui) : IScene
{
    public void Update(GameTime gameTime) => input.Update(gameTime);

    public void Draw(GameTime gameTime) => ui.Draw();

    public void Dispose() { }

    public static void ConfigureServices(ContainerBuilder builder)
    {
        builder.RegisterType<PauseInputManager>();
        builder.RegisterType<PauseUi>();

        // Register the scene itself
        builder.RegisterType<PauseMenuScene>();
    }
}