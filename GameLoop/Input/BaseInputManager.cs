using System;
using System.Linq;
using GameLoop.Scenes;
using Gameplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

/// <summary>
///     Contains any global input logic, eg exit
/// </summary>
internal abstract class BaseInputManager(
    IGlobalCommands commands,
    GameFocusState focusState,
    SceneManager sceneManager)
{
    internal InputMethod CurrentInputMethod { get; private set; } = InputMethod.KeyboardMouse;

    protected IGlobalCommands GlobalCommands { get; } = commands;

    protected KeyboardState KeyboardState { get; private set; } = Keyboard.GetState();
    protected KeyboardState PreviousKeyboardState { get; private set; } = Keyboard.GetState();
    protected GamePadState GamePadState { get; private set; } = GamePad.GetState(0);
    protected GamePadState PreviousGamePadState { get; set; }
    protected MouseState MouseState { get; private set; } = Mouse.GetState();
    protected MouseState PreviousMouseState { get; private set; } = Mouse.GetState();

    protected bool ShouldSkipInput()
    {
        if (sceneManager.InputFramesToSkip <= 0) return false;

        sceneManager.InputFramesToSkip--;
        return true;
    }

    internal virtual void Update(GameTime gameTime)
    {
        PreviousKeyboardState = KeyboardState;
        KeyboardState = Keyboard.GetState();
        PreviousGamePadState = GamePadState;
        GamePadState = GamePad.GetState(0);

        if (focusState.HasFocus)
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

    protected bool WasPressedThisFrame(Keys key) =>
        KeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyUp(key);

    protected bool WasPressedThisFrame(Buttons button) =>
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