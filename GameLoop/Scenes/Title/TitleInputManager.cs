using GameLoop.Input;
using Gameplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.Title;

internal class TitleInputManager(IGlobalCommands globalCommands, GameFocusState focusState, SceneManager sceneManager)
    : BaseInputManager(globalCommands, focusState, sceneManager)
{
    internal override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (ShouldSkipInput()) return;

        if (WasPressedThisFrame(Keys.Escape) || WasPressedThisFrame(Buttons.Back) || WasPressedThisFrame(Buttons.B))
            GlobalCommands.Exit();
        if (WasPressedThisFrame(Keys.Enter) || WasPressedThisFrame(Buttons.Start))
            GlobalCommands.StartGame();
    }
}