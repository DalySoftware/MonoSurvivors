using System;
using GameLoop.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

public enum PauseAction
{
    Resume,
    Activate,
}

internal sealed class PauseActionInput(GameInputState state)
{
    private readonly TimeSpan _navCooldown = TimeSpan.FromMilliseconds(150);
    private TimeSpan _currentNavCooldown = TimeSpan.Zero;

    public bool WasPressed(PauseAction action) => action switch
    {
        PauseAction.Resume =>
            IsPressed(Keys.Escape) || IsPressed(Buttons.Start) || IsPressed(Buttons.B),

        PauseAction.Activate => state.CurrentInputMethod switch
        {
            InputMethod.KeyboardMouse => IsLeftMouseClicked() || IsPressed(Keys.Enter),
            InputMethod.Gamepad => IsPressed(Buttons.A),
            _ => false,
        },

        _ => false,
    };

    public Direction? GetNavigationDirection(GameTime gameTime)
    {
        _currentNavCooldown -= gameTime.ElapsedGameTime;
        if (_currentNavCooldown > TimeSpan.Zero)
            return null;

        var stick = state.GamePadState.ThumbSticks.Left;

        const float deadzone = 0.2f;
        stick.X = MathF.Abs(stick.X) >= deadzone ? stick.X : 0f;
        stick.Y = MathF.Abs(stick.Y) >= deadzone ? stick.Y : 0f;

        Direction? direction = (stick.X, stick.Y) switch
        {
            (_, > 0.5f) => Direction.Up,
            (_, < -0.5f) => Direction.Down,
            (< -0.5f, _) => Direction.Left,
            (> 0.5f, _) => Direction.Right,
            _ => null,
        };

        // Edge-triggered D-pad
        if (IsPressed(Buttons.DPadUp)) direction = Direction.Up;
        if (IsPressed(Buttons.DPadDown)) direction = Direction.Down;
        if (IsPressed(Buttons.DPadLeft)) direction = Direction.Left;
        if (IsPressed(Buttons.DPadRight)) direction = Direction.Right;

        if (direction != null)
            _currentNavCooldown = _navCooldown;

        return direction;
    }

    private bool IsPressed(Keys key) =>
        state.KeyboardState.IsKeyDown(key) && state.PreviousKeyboardState.IsKeyUp(key);

    private bool IsPressed(Buttons button) =>
        state.GamePadState.IsButtonDown(button) && state.PreviousGamePadState.IsButtonUp(button);

    private bool IsLeftMouseClicked() =>
        state.MouseState.LeftButton == ButtonState.Pressed &&
        state.PreviousMouseState.LeftButton == ButtonState.Released;

    public bool WasLeftMousePressedThisFrame() =>
        state.MouseState.LeftButton == ButtonState.Pressed &&
        state.PreviousMouseState.LeftButton == ButtonState.Released;

    public bool WasLeftMouseReleasedThisFrame() =>
        state.MouseState.LeftButton == ButtonState.Released &&
        state.PreviousMouseState.LeftButton == ButtonState.Pressed;

    public Vector2? GetPointerPosition()
    {
        if (state.CurrentInputMethod != InputMethod.KeyboardMouse)
            return null;

        return state.MouseState.Position.ToVector2();
    }
}