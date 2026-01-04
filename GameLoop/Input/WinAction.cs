using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

internal enum WinAction
{
    Exit,
    StartGame,
}

internal sealed class WinActionInput(GameInputState state)
{
    public bool WasPressed(WinAction action) => action switch
    {
        WinAction.Exit => IsPressed(Keys.Escape) || IsPressed(Buttons.Back),
        WinAction.StartGame => IsPressed(Keys.Space) || IsPressed(Buttons.Start),
        _ => false,
    };

    private bool IsPressed(Keys key) =>
        state.KeyboardState.IsKeyDown(key) &&
        state.PreviousKeyboardState.IsKeyUp(key);

    private bool IsPressed(Buttons button) =>
        state.GamePadState.IsButtonDown(button) &&
        state.PreviousGamePadState.IsButtonUp(button);
}