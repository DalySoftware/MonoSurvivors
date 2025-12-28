using System;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

internal sealed class GameInputState
{
    private bool _hasFocus;
    internal KeyboardState KeyboardState { get; set; } = Keyboard.GetState();
    internal KeyboardState PreviousKeyboardState { get; set; } = Keyboard.GetState();
    internal GamePadState GamePadState { get; set; } = GamePad.GetState(0);
    internal GamePadState PreviousGamePadState { get; set; }
    internal MouseState MouseState { get; set; } = Mouse.GetState();
    internal MouseState PreviousMouseState { get; set; } = Mouse.GetState();

    public bool HasFocus
    {
        get => _hasFocus;
        set
        {
            var gainedFocus = value && !_hasFocus;
            if (gainedFocus) GainedFocus?.Invoke();

            _hasFocus = value;
        }
    }

    public InputMethod CurrentInputMethod { get; set; }

    public event Action? GainedFocus;
}

internal enum InputMethod
{
    KeyboardMouse,
    Gamepad,
}