using GameLoop.Input;
using Gameplay;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.GameOver;

internal class GameOverInputManager(IGlobalCommands globalCommands) : BaseInputManager
{
    internal override void Update()
    {
        base.Update();

        if (WasPressedThisFrame(Keys.Escape) || GamePadState.Buttons.Back == ButtonState.Pressed) globalCommands.Exit();
        if (KeyboardState.IsKeyDown(Keys.Space) || GamePadState.Buttons.Start == ButtonState.Pressed)
            globalCommands.StartGame();
    }
}