using System;
using System.Linq;
using GameLoop.Input.Mapping;
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
}

internal sealed class PauseActionInput(GameInputState state, KeyBindingsSettings bindings)
{
    private readonly TimeSpan _navCooldown = TimeSpan.FromMilliseconds(150);
    private TimeSpan _currentNavCooldown = TimeSpan.Zero;

    private ActionKeyMap<PauseAction> Map => bindings.PauseMenuActions;

    public bool WasPressed(PauseAction action) =>
        Map.GetKeys(action).Any(IsPressed) || Map.GetButtons(action).Any(IsPressed);

    public bool IsDown(PauseAction action) => Map.GetKeys(action).Any(k => state.KeyboardState.IsKeyDown(k)) ||
                                              Map.GetButtons(action).Any(b => state.GamePadState.IsButtonDown(b));

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
        if (IsDown(PauseAction.NavigateUp)) direction = Direction.Up;
        if (IsDown(PauseAction.NavigateDown)) direction = Direction.Down;
        if (IsDown(PauseAction.NavigateLeft)) direction = Direction.Left;
        if (IsDown(PauseAction.NavigateRight)) direction = Direction.Right;

        if (direction != null)
            _currentNavCooldown = _navCooldown;

        return direction;
    }

    private bool IsPressed(Keys key) =>
        state.KeyboardState.IsKeyDown(key) && state.PreviousKeyboardState.IsKeyUp(key);

    private bool IsPressed(Buttons button) =>
        state.GamePadState.IsButtonDown(button) && state.PreviousGamePadState.IsButtonUp(button);

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