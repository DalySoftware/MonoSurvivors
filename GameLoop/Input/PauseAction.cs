using System;
using GameLoop.UI;
using GameLoop.UserSettings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

public enum PauseAction
{
    Resume,
    Activate,
    NavigateUp,
    NavigateDown,
    NavigateLeft,
    NavigateRight,
    ToggleFullscreen,
}

internal sealed class PauseActionInput(GameInputState state, KeyBindingsSettings bindings)
    : ActionInputBase<PauseAction>(state, bindings.PauseMenuActions)
{
    private readonly TimeSpan _navCooldown = TimeSpan.FromMilliseconds(150);
    private TimeSpan _currentNavCooldown = TimeSpan.Zero;

    public Direction? GetNavigationDirection(GameTime gameTime)
    {
        _currentNavCooldown -= gameTime.ElapsedGameTime;
        if (_currentNavCooldown > TimeSpan.Zero)
            return null;

        var stick = State.GamePadState.ThumbSticks.Left;
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
        if (IsDown(PauseAction.NavigateUp)) direction = Direction.Up;
        if (IsDown(PauseAction.NavigateDown)) direction = Direction.Down;
        if (IsDown(PauseAction.NavigateLeft)) direction = Direction.Left;
        if (IsDown(PauseAction.NavigateRight)) direction = Direction.Right;

        if (direction != null)
            _currentNavCooldown = _navCooldown;

        return direction;
    }

    public bool WasLeftMousePressedThisFrame() =>
        State.MouseState.LeftButton == ButtonState.Pressed &&
        State.PreviousMouseState.LeftButton == ButtonState.Released;

    public bool WasLeftMouseReleasedThisFrame() =>
        State.MouseState.LeftButton == ButtonState.Released &&
        State.PreviousMouseState.LeftButton == ButtonState.Pressed;

    public Vector2? GetPointerPosition()
    {
        if (State.CurrentInputMethod != InputMethod.KeyboardMouse)
            return null;

        return State.MouseState.Position.ToVector2();
    }
}