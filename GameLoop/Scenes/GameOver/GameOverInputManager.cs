using GameLoop.Input;
using Gameplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.GameOver;

internal class GameOverInputManager(
    IGlobalCommands globalCommands,
    GameFocusState focusState,
    SceneManager sceneManager)
    : BaseInputManager(globalCommands, focusState, sceneManager)
{
    internal override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (ShouldSkipInput()) return;

        if (WasPressedThisFrame(Keys.Escape) || WasPressedThisFrame(Buttons.Back)) GlobalCommands.Exit();
        if (KeyboardState.IsKeyDown(Keys.Space) || WasPressedThisFrame(Buttons.Start))
            GlobalCommands.StartGame();
    }
}