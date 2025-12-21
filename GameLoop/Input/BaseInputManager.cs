using System;
using System.Linq;
using Gameplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

/// <summary>
///     Contains any global input logic, eg exit
/// </summary>
internal abstract class BaseInputManager(IGlobalCommands commands)
{
    protected IGlobalCommands GlobalCommands { get; } = commands;

    internal static bool GameHasFocus
    {
        get;
        set
        {
            var gainedFocus = value && !field;
            if (gainedFocus)
            {
                // reset mouse state
                MouseState = Mouse.GetState();
                PreviousMouseState = MouseState;
            }

            field = value;
        }
    }

    internal InputMethod CurrentInputMethod { get; private set; } = InputMethod.KeyboardMouse;

    protected static KeyboardState KeyboardState { get; private set; } = Keyboard.GetState();
    protected static KeyboardState PreviousKeyboardState { get; private set; } = Keyboard.GetState();
    protected static GamePadState GamePadState { get; private set; } = GamePad.GetState(0);
    protected static GamePadState PreviousGamePadState { get; set; }
    protected static MouseState MouseState { get; private set; } = Mouse.GetState();
    protected static MouseState PreviousMouseState { get; private set; } = Mouse.GetState();

    internal virtual void Update(GameTime gameTime)
    {
        PreviousKeyboardState = KeyboardState;
        KeyboardState = Keyboard.GetState();
        PreviousGamePadState = GamePadState;
        GamePadState = GamePad.GetState(0);

        if (GameHasFocus)
        {
            PreviousMouseState = MouseState;
            MouseState = Mouse.GetState();
        }

        UpdateInputMethod();
    }

    private void UpdateInputMethod()
    {
        if (GamePadState.IsConnected && IsAnyButtonPressed(GamePadState))
        {
            CurrentInputMethod = InputMethod.Gamepad;
            GlobalCommands.HideMouse();
            return;
        }

        if (Vector2.DistanceSquared(MouseState.Position.ToVector2(), PreviousMouseState.Position.ToVector2()) > 1f)
        {
            CurrentInputMethod = InputMethod.KeyboardMouse;
            GlobalCommands.ShowMouse();
        }
    }

    protected static bool WasPressedThisFrame(Keys key) =>
        KeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyUp(key);

    protected static bool WasPressedThisFrame(Buttons button) =>
        GamePadState.IsButtonDown(button) && PreviousGamePadState.IsButtonUp(button);

    private bool IsAnyButtonPressed(GamePadState state)
    {
        if (!state.IsConnected) return false;

        var buttonsToCheck = Enum.GetValues<Buttons>();

        return buttonsToCheck.Any(button => state.IsButtonDown(button));
    }
}

internal enum InputMethod
{
    KeyboardMouse,
    Gamepad,
}