using System;
using GameLoop.Input;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.Pause;

internal class PauseInputManager : BaseInputManager
{
    internal Action OnResume { get; init; } = () => { };

    internal override void Update()
    {
        base.Update();

        if (WasPressedThisFrame(Keys.Escape) || GamePadState.Buttons.Start == ButtonState.Pressed)
        {
            OnResume();
        }
    }
}
