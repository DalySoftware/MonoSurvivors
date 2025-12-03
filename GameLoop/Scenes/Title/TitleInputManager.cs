using System;
using GameLoop.Input;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.Title;

internal class TitleInputManager : BaseInputManager
{
    internal Action OnStartGame { get; init; } = () => { };

    internal override void Update()
    {
        base.Update();

        var keyboardState = Keyboard.GetState();
        var gamePadState = GamePad.GetState(0);

        if (keyboardState.IsKeyDown(Keys.Enter) || gamePadState.Buttons.Start == ButtonState.Pressed) OnStartGame();
    }
}