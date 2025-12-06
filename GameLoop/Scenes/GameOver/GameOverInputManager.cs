using System;
using GameLoop.Input;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.GameOver;

internal class GameOverInputManager : BaseInputManager
{
    internal Action OnRestart { get; init; } = () => { };

    internal override void Update()
    {
        base.Update();

        var keyboardState = Keyboard.GetState();
        var gamePadState = GamePad.GetState(0);

        if (keyboardState.IsKeyDown(Keys.Space) || gamePadState.Buttons.Start == ButtonState.Pressed)
        {
            OnRestart();
        }
    }
}
