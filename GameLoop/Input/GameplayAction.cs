using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

internal enum GameplayAction
{
    Pause,
    OpenSphereGrid,
    MoveLeft,
    MoveRight,
    MoveUp,
    MoveDown,
}

internal sealed class GameplayActionInput(GameInputState state)
{
    public bool WasPressed(GameplayAction action) => action switch
    {
        GameplayAction.Pause => IsPressed(Keys.Escape) || IsPressed(Buttons.Start),
        GameplayAction.OpenSphereGrid => IsPressed(Keys.Space) || IsPressed(Keys.Tab) || IsPressed(Buttons.Back),
        _ => false,
    };

    public bool IsDown(GameplayAction action) => action switch
    {
        GameplayAction.MoveLeft => state.KeyboardState.IsKeyDown(Keys.S),
        GameplayAction.MoveRight => state.KeyboardState.IsKeyDown(Keys.F),
        GameplayAction.MoveUp => state.KeyboardState.IsKeyDown(Keys.E),
        GameplayAction.MoveDown => state.KeyboardState.IsKeyDown(Keys.D),
        _ => false,
    };

    private bool IsPressed(Keys key) =>
        state.KeyboardState.IsKeyDown(key) &&
        state.PreviousKeyboardState.IsKeyUp(key);

    private bool IsPressed(Buttons button) =>
        state.GamePadState.IsButtonDown(button) &&
        state.PreviousGamePadState.IsButtonUp(button);

    public Vector2 GetMovement()
    {
        var x = 0f;
        var y = 0f;

        if (IsDown(GameplayAction.MoveLeft)) x -= 1f;
        if (IsDown(GameplayAction.MoveRight)) x += 1f;
        if (IsDown(GameplayAction.MoveUp)) y -= 1f;
        if (IsDown(GameplayAction.MoveDown)) y += 1f;

        if (state.GamePadState.IsConnected)
        {
            var stick = state.GamePadState.ThumbSticks.Left;

            const float deadzone = 0.2f;

            if (MathF.Abs(stick.X) >= deadzone)
                x += stick.X;

            if (MathF.Abs(stick.Y) >= deadzone)
                y -= stick.Y;
        }

        return new Vector2(x, y);
    }
}