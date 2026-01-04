using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

internal enum GameOverAction
{
    Exit,
    StartGame,
}

internal sealed class GameOverActionInput(GameInputState state)
{
    public bool WasPressed(GameOverAction action) => action switch
    {
        GameOverAction.Exit => IsPressed(Keys.Escape) || IsPressed(Buttons.Back),
        GameOverAction.StartGame => IsPressed(Keys.Space) || IsPressed(Buttons.Start),
        _ => false,
    };

    private bool IsPressed(Keys key) =>
        state.KeyboardState.IsKeyDown(key) &&
        state.PreviousKeyboardState.IsKeyUp(key);

    private bool IsPressed(Buttons button) =>
        state.GamePadState.IsButtonDown(button) &&
        state.PreviousGamePadState.IsButtonUp(button);
}