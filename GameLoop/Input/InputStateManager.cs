using System;
using System.Linq;
using Gameplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

internal class InputStateManager(GameInputState state, IGlobalCommands globalCommands)
{
    public void Update(bool hasFocus)
    {
        state.PreviousKeyboardState = state.KeyboardState;
        state.KeyboardState = Keyboard.GetState();
        state.PreviousGamePadState = state.GamePadState;
        state.GamePadState = GamePad.GetState(0);

        state.HasFocus = hasFocus;
        if (state.HasFocus)
        {
            state.PreviousMouseState = state.MouseState;
            state.MouseState = Mouse.GetState();
        }

        UpdateInputMethod();
    }

    private void UpdateInputMethod()
    {
        // Gamepad activity
        if (IsGamePadActive())
        {
            state.CurrentInputMethod = InputMethod.Gamepad;
            globalCommands.HideMouse();
            return;
        }

        // Keyboard or mouse activity
        if (IsKeyboardActive() || IsMouseActive())
        {
            state.CurrentInputMethod = InputMethod.KeyboardMouse;
            globalCommands.ShowMouse();
        }
    }

    private bool IsGamePadActive()
    {
        if (!state.GamePadState.IsConnected) return false;

        var current = state.GamePadState;
        var previous = state.PreviousGamePadState;

        if (Enum.GetValues<Buttons>()
            .Any(button => current.IsButtonDown(button) && previous.IsButtonUp(button))) return true;

        const float deadzone = 0.2f;
        return current.ThumbSticks.Left.Length() > deadzone ||
               current.ThumbSticks.Right.Length() > deadzone ||
               current.Triggers.Left > deadzone ||
               current.Triggers.Right > deadzone;
    }

    private bool IsKeyboardActive() =>
        state.KeyboardState.GetPressedKeys().Any(key => state.PreviousKeyboardState.IsKeyUp(key));

    private bool IsMouseActive()
    {
        var current = state.MouseState;
        var previous = state.PreviousMouseState;

        if (Vector2.DistanceSquared(current.Position.ToVector2(), previous.Position.ToVector2()) > 1f)
            return true;

        if (current.ScrollWheelValue != previous.ScrollWheelValue)
            return true;

        return current.LeftButton != previous.LeftButton ||
               current.RightButton != previous.RightButton ||
               current.MiddleButton != previous.MiddleButton;
    }
}