using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

internal enum SphereGridAction
{
    Close,
    UnlockHovered,
    UnlockFocused,
    ResetCamera,
}

internal sealed class SphereGridActionInput(GameInputState state)
{
    private readonly TimeSpan _navigationCooldown = TimeSpan.FromMilliseconds(150);
    private TimeSpan _currentNavigationCooldown = TimeSpan.Zero;

    public bool WasPressed(SphereGridAction action) => action switch
    {
        SphereGridAction.Close =>
            IsPressed(Keys.Escape) ||
            IsPressed(Keys.Space) ||
            IsPressed(Keys.Tab) ||
            IsPressed(Buttons.Back) ||
            IsPressed(Buttons.B),

        SphereGridAction.UnlockFocused =>
            IsPressed(Buttons.A),

        SphereGridAction.UnlockHovered =>
            WasLeftMousePressedThisFrame() &&
            state.CurrentInputMethod == InputMethod.KeyboardMouse, // optional guard

        SphereGridAction.ResetCamera =>
            IsPressed(Keys.T),

        _ => false,
    };

    // --- Mouse semantics ---
    public bool IsMousePanning() =>
        state.MouseState.MiddleButton == ButtonState.Pressed;

    public Vector2 GetMousePanDelta() =>
        new(
            state.MouseState.X - state.PreviousMouseState.X,
            state.MouseState.Y - state.PreviousMouseState.Y
        );

    public Vector2? GetMousePositionForHover()
    {
        if (state.CurrentInputMethod != InputMethod.KeyboardMouse)
            return null;

        return state.MouseState.Position.ToVector2();
    }

    public bool WasLeftMousePressedThisFrame() =>
        state.MouseState.LeftButton == ButtonState.Pressed &&
        state.PreviousMouseState.LeftButton == ButtonState.Released;

    public Vector2 GetRightStickPan()
    {
        var stick = state.GamePadState.ThumbSticks.Right;
        return stick.LengthSquared() > 0.02f
            ? new Vector2(-stick.X, stick.Y)
            : Vector2.Zero;
    }

    public Vector2 GetNavigationDirection(GameTime gameTime)
    {
        // Left thumbstick
        var stick = state.GamePadState.ThumbSticks.Left;
        stick.Y *= -1f;

        if (stick.LengthSquared() >= 0.02f)
        {
            _currentNavigationCooldown -= gameTime.ElapsedGameTime;
            if (_currentNavigationCooldown > TimeSpan.Zero)
                return Vector2.Zero;

            _currentNavigationCooldown = _navigationCooldown;
            return stick;
        }

        // Reset cooldown when stick released
        _currentNavigationCooldown = TimeSpan.Zero;

        // D-pad (edge-triggered)
        var direction = Vector2.Zero;

        if (IsPressed(Buttons.DPadUp)) direction += Vector2.UnitY * -1f;
        if (IsPressed(Buttons.DPadDown)) direction += Vector2.UnitY;
        if (IsPressed(Buttons.DPadLeft)) direction += Vector2.UnitX * -1f;
        if (IsPressed(Buttons.DPadRight)) direction += Vector2.UnitX;

        return direction;
    }


    private bool IsPressed(Keys key) =>
        state.KeyboardState.IsKeyDown(key) &&
        state.PreviousKeyboardState.IsKeyUp(key);

    private bool IsPressed(Buttons button) =>
        state.GamePadState.IsButtonDown(button) &&
        state.PreviousGamePadState.IsButtonUp(button);
}