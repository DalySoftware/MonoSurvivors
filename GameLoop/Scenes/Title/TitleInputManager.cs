using System;
using GameLoop.Input;
using Gameplay;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.Title;

internal class TitleInputManager(IGlobalCommands globalCommands) : BaseInputManager
{
    internal Action OnStartGame { get; init; } = () => { };

    internal override void Update()
    {
        base.Update();

        if (WasPressedThisFrame(Keys.Escape) || GamePadState.Buttons.Back == ButtonState.Pressed) globalCommands.Exit();
        if (WasPressedThisFrame(Keys.Enter) || GamePadState.Buttons.Start == ButtonState.Pressed)
            globalCommands.StartGame();
    }
}