using System;
using GameLoop.Input.Mapping;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

internal enum SphereGridAction
{
    Close,
    UnlockHovered,
    UnlockFocused,
    ResetCamera,
    NavigateUp,
    NavigateDown,
    NavigateLeft,
    NavigateRight,
}

internal sealed class SphereGridActionInput(GameInputState state, ActionKeyMap<SphereGridAction> map)
    : ActionInputBase<SphereGridAction>(state, map)
{
    private readonly TimeSpan _navigationCooldown = TimeSpan.FromMilliseconds(150);
    private TimeSpan _currentNavigationCooldown = TimeSpan.Zero;

    public bool IsActionTriggered(SphereGridAction action) => action switch
    {
        // Mouse-only semantic
        SphereGridAction.UnlockHovered
            => WasLeftMousePressedThisFrame() &&
               State.CurrentInputMethod == InputMethod.KeyboardMouse,

        _ => WasPressed(action),
    };

    // --- Mouse semantics ---
    public bool IsMousePanning() =>
        State.MouseState.MiddleButton == ButtonState.Pressed;

    public Vector2 GetMousePanDelta() =>
        new(
            State.MouseState.X - State.PreviousMouseState.X,
            State.MouseState.Y - State.PreviousMouseState.Y
        );

    public Vector2? GetMousePositionForHover()
    {
        if (State.CurrentInputMethod != InputMethod.KeyboardMouse)
            return null;

        return State.MouseState.Position.ToVector2();
    }

    private bool WasLeftMousePressedThisFrame() =>
        State.MouseState.LeftButton == ButtonState.Pressed &&
        State.PreviousMouseState.LeftButton == ButtonState.Released;

    public Vector2 GetRightStickPan()
    {
        var stick = State.GamePadState.ThumbSticks.Right;
        return stick.LengthSquared() > 0.02f
            ? new Vector2(-stick.X, stick.Y)
            : Vector2.Zero;
    }

    public Vector2 GetNavigationDirection(GameTime gameTime)
    {
        // Left thumbstick (rate-limited)
        var stick = State.GamePadState.ThumbSticks.Left;
        stick.Y *= -1f;

        if (stick.LengthSquared() >= 0.02f)
        {
            _currentNavigationCooldown -= gameTime.ElapsedGameTime;
            if (_currentNavigationCooldown > TimeSpan.Zero)
                return Vector2.Zero;

            _currentNavigationCooldown = _navigationCooldown;
            return stick;
        }

        _currentNavigationCooldown = TimeSpan.Zero;

        // D-pad (edge-triggered)
        var direction = Vector2.Zero;

        if (IsDown(SphereGridAction.NavigateUp)) direction += Vector2.UnitY * -1f;
        if (IsDown(SphereGridAction.NavigateDown)) direction += Vector2.UnitY;
        if (IsDown(SphereGridAction.NavigateLeft)) direction += Vector2.UnitX * -1f;
        if (IsDown(SphereGridAction.NavigateRight)) direction += Vector2.UnitX;

        return direction;
    }
}