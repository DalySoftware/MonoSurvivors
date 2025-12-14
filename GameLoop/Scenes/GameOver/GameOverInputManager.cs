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

        if (WasPressedThisFrame(Keys.Escape) || GamePadState.Buttons.Back == ButtonState.Pressed) OnExit();
        if (KeyboardState.IsKeyDown(Keys.Space) || GamePadState.Buttons.Start == ButtonState.Pressed) OnRestart();
    }
}