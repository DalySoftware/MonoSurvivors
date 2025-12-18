using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

/// <summary>
///     Contains any global input logic, eg exit
/// </summary>
internal abstract class BaseInputManager
{
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


    protected static KeyboardState KeyboardState { get; private set; } = Keyboard.GetState();
    protected static KeyboardState PreviousKeyboardState { get; private set; } = Keyboard.GetState();
    protected static GamePadState GamePadState { get; private set; } = GamePad.GetState(0);
    protected static MouseState MouseState { get; private set; } = Mouse.GetState();
    protected static MouseState PreviousMouseState { get; private set; } = Mouse.GetState();

    internal virtual void Update()
    {
        PreviousKeyboardState = KeyboardState;
        KeyboardState = Keyboard.GetState();
        GamePadState = GamePad.GetState(0);

        if (GameHasFocus)
        {
            PreviousMouseState = MouseState;
            MouseState = Mouse.GetState();
        }
    }

    protected static bool WasPressedThisFrame(Keys key) =>
        KeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyUp(key);
}