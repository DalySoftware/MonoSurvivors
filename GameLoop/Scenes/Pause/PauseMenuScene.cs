using Autofac;
using GameLoop.Input;
using GameLoop.Scenes.Pause.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Pause;

internal class PauseMenuScene(PauseInputManager input, InputGate inputGate, PauseUi ui, SpriteBatch spriteBatch)
    : IScene
{
    public void Update(GameTime gameTime)
    {
        if (inputGate.ShouldProcessInput())
            input.Update(gameTime);
    }

    public void Draw(GameTime gameTime) => ui.Draw(spriteBatch);

    public void Dispose() { }

    public static void ConfigureServices(ContainerBuilder builder)
    {
        builder.RegisterType<PauseInputManager>().SingleInstance();
        builder.RegisterType<PauseUi>().SingleInstance();

        // Register the scene itself
        builder.RegisterType<PauseMenuScene>();
    }
}