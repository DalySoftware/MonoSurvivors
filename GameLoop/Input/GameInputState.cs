using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

internal sealed class GameInputState
{
    internal KeyboardState KeyboardState { get; set; } = Keyboard.GetState();
    internal KeyboardState PreviousKeyboardState { get; set; } = Keyboard.GetState();
    internal GamePadState GamePadState { get; set; } = GamePad.GetState(0);
    internal GamePadState PreviousGamePadState { get; set; }
    internal MouseState MouseState { get; set; } = Mouse.GetState();
    internal MouseState PreviousMouseState { get; set; } = Mouse.GetState();

    public InputMethod CurrentInputMethod { get; set; }
}

internal enum InputMethod
{
    KeyboardMouse,
    Gamepad,
}