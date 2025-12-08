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

        if (WasPressedThisFrame(Keys.Enter) || GamePadState.Buttons.Start == ButtonState.Pressed) OnStartGame();
    }
}