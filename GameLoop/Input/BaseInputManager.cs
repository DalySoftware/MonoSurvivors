using System;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

/// <summary>
///     Contains any global input logic, eg exit
/// </summary>
internal abstract class BaseInputManager
{
    internal Action OnExit { get; init; } = () => { };

    internal virtual void Update()
    {
        var keyboardState = Keyboard.GetState();
        var gamePadState = GamePad.GetState(0);

        if (keyboardState.IsKeyDown(Keys.Escape) || gamePadState.Buttons.Back == ButtonState.Pressed) OnExit();
    }
}