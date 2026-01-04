using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

internal enum SingleActionSceneAction
{
    Exit,
    PrimaryAction,
}

internal sealed class SingleActionSceneActionInput(GameInputState state)
{
    public bool WasPressed(SingleActionSceneAction action) => action switch
    {
        SingleActionSceneAction.Exit =>
            IsPressed(Keys.Escape) || IsPressed(Buttons.Back),

        SingleActionSceneAction.PrimaryAction => state.CurrentInputMethod switch
        {
            InputMethod.KeyboardMouse => IsPressed(Keys.Space) || IsPressed(Keys.Enter),
            InputMethod.Gamepad => IsPressed(Buttons.Start) || IsPressed(Buttons.A),
            _ => false,
        },

        _ => false,
    };

    private bool IsPressed(Keys key) =>
        state.KeyboardState.IsKeyDown(key) &&
        state.PreviousKeyboardState.IsKeyUp(key);

    private bool IsPressed(Buttons button) =>
        state.GamePadState.IsButtonDown(button) &&
        state.PreviousGamePadState.IsButtonUp(button);
}