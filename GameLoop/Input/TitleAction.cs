using System;
using GameLoop.UserSettings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

public enum TitleAction
{
    StartGame,
    Exit,
    NextWeapon,
    PreviousWeapon,
}

internal sealed class TitleActionInput(GameInputState state, KeyBindingsSettings bindings)
    : ActionInputBase<TitleAction>(state, bindings.TitleActions)
{
    private readonly TimeSpan _navigationCooldown = TimeSpan.FromMilliseconds(250);
    private TimeSpan _currentNavigationCooldown = TimeSpan.Zero;

    public TitleAction? GetThumbstickNavigation(GameTime gameTime)
    {
        _currentNavigationCooldown -= gameTime.ElapsedGameTime;
        if (_currentNavigationCooldown > TimeSpan.Zero)
            return null;

        var stick = State.GamePadState.ThumbSticks.Left;

        const float deadzone = 0.2f;
        if (MathF.Abs(stick.X) < deadzone) return null;

        _currentNavigationCooldown = _navigationCooldown;
        return stick.X > 0 ? TitleAction.NextWeapon : TitleAction.PreviousWeapon;
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