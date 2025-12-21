using System;
using GameLoop.Input;
using Gameplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.Title;

internal class TitleInputManager(IGlobalCommands globalCommands, GameFocusState focusState)
    : BaseInputManager(globalCommands, focusState)
{
    internal Action OnStartGame { get; init; } = () => { };

    internal override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (WasPressedThisFrame(Keys.Escape) || WasPressedThisFrame(Buttons.Back) || WasPressedThisFrame(Buttons.B))
            GlobalCommands.Exit();
        if (WasPressedThisFrame(Keys.Enter) || WasPressedThisFrame(Buttons.Start))
            GlobalCommands.StartGame();
    }
}