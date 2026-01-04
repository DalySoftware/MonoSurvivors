using System;
using System.Linq;
using GameLoop.UserSettings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

public enum GameplayAction
{
    Pause,
    OpenSphereGrid,
    MoveLeft,
    MoveRight,
    MoveUp,
    MoveDown,
}

internal sealed class GameplayActionInput(GameInputState state, KeyBindingsSettings bindings)
{
    // Discrete actions, including discrete movement
    public bool WasPressed(GameplayAction action)
    {
        var kbKeys = bindings.GameplayActions.Keyboard[action];
        var gpButtons = bindings.GameplayActions.Gamepad[action];

        return kbKeys.Any(WasPressed) || gpButtons.Any(WasPressed);
    }

    // Continuous check (for movement only)
    public bool IsDown(GameplayAction action) =>
        bindings
            .GameplayActions
            .Keyboard[action]
            .Any(k => state.KeyboardState.IsKeyDown(k)) ||
        bindings
            .GameplayActions
            .Gamepad[action]
            .Any(b => state.GamePadState.IsButtonDown(b));

    private bool WasPressed(Keys key) =>
        state.KeyboardState.IsKeyDown(key) &&
        state.PreviousKeyboardState.IsKeyUp(key);

    private bool WasPressed(Buttons button) =>
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