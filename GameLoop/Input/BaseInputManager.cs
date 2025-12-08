using System;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

/// <summary>
///     Contains any global input logic, eg exit
/// </summary>
internal abstract class BaseInputManager
{
    protected KeyboardState KeyboardState { get; private set; } = Keyboard.GetState();
    protected KeyboardState PreviousKeyboardState { get; private set; } = Keyboard.GetState();
    protected GamePadState GamePadState { get; private set; } = GamePad.GetState(0);
    
    internal Action OnExit { get; init; } = () => { };

    internal virtual void Update()
    {
        PreviousKeyboardState = KeyboardState;
        KeyboardState = Keyboard.GetState();
        GamePadState = GamePad.GetState(0);

        if (KeyboardState.IsKeyDown(Keys.Escape) || GamePadState.Buttons.Back == ButtonState.Pressed) OnExit();
    }
    
    protected bool WasPressedThisFrame(Keys key) => 
        KeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyUp(key);
}