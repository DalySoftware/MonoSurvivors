using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

internal enum TitleAction
{
    Exit,
    StartGame,
}

internal sealed class TitleActionInput(GameInputState state)
{
    public bool WasPressed(TitleAction action) => action switch
    {
        TitleAction.Exit => IsPressed(Keys.Escape) || IsPressed(Buttons.Back) || IsPressed(Buttons.B),
        TitleAction.StartGame => IsPressed(Keys.Enter) || IsPressed(Buttons.Start),
        _ => false,
    };

    private bool IsPressed(Keys key) =>
        state.KeyboardState.IsKeyDown(key) &&
        state.PreviousKeyboardState.IsKeyUp(key);

    private bool IsPressed(Buttons button) =>
        state.GamePadState.IsButtonDown(button) &&
        state.PreviousGamePadState.IsButtonUp(button);
}