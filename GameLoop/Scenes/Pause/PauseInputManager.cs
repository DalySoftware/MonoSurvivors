using System;
using GameLoop.Input;
using Gameplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.Pause;

internal class PauseInputManager(IGlobalCommands globalCommands) : BaseInputManager
{
    private readonly Action _onResume = globalCommands.ResumeGame;

    internal override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (WasPressedThisFrame(Keys.Escape) ||
            WasPressedThisFrame(Buttons.Start) ||
            WasPressedThisFrame(Buttons.B)) _onResume();
    }
}