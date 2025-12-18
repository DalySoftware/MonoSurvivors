using System;
using GameLoop.Input;
using Gameplay;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.Pause;

internal class PauseInputManager(IGlobalCommands globalCommands) : BaseInputManager
{
    private readonly Action _onResume = globalCommands.ResumeGame;

    internal override void Update()
    {
        base.Update();

        if (WasPressedThisFrame(Keys.Escape) || GamePadState.Buttons.Start == ButtonState.Pressed) _onResume();
    }
}