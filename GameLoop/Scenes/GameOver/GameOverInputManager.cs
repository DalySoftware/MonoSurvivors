using GameLoop.Input;
using Gameplay;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.GameOver;

internal class GameOverInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState,
    SceneManager sceneManager)
    : BaseInputManager(globalCommands, inputState, sceneManager)
{
    internal void Update()
    {
        if (ShouldSkipInput()) return;

        if (WasPressedThisFrame(Keys.Escape) || WasPressedThisFrame(Buttons.Back)) GlobalCommands.Exit();
        if (InputState.KeyboardState.IsKeyDown(Keys.Space) || WasPressedThisFrame(Buttons.Start))
            GlobalCommands.StartGame();
    }
}