using GameLoop.Scenes.Pause.UI;
using GameLoop.UI;
using Microsoft.Xna.Framework;

namespace GameLoop.Scenes.Pause;

internal class PauseMenuScene(PauseInputManager input, PauseUi ui) : IScene
{
    public void Update(GameTime gameTime)
    {
        input.Update();
        ui.Update();
    }

    public void Draw(GameTime gameTime) => ui.Draw();

    public void Dispose() { }
}