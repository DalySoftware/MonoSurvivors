using GameLoop.Input;
using Gameplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.Title;

internal class TitleInputManager(IGlobalCommands globalCommands, GameInputState inputState, SceneManager sceneManager)
    : BaseInputManager(globalCommands, inputState, sceneManager)
{
    internal void Update(GameTime gameTime)
    {
        if (ShouldSkipInput()) return;

        if (WasPressedThisFrame(Keys.Escape) || WasPressedThisFrame(Buttons.Back) || WasPressedThisFrame(Buttons.B))
            GlobalCommands.Exit();
        if (WasPressedThisFrame(Keys.Enter) || WasPressedThisFrame(Buttons.Start))
            GlobalCommands.StartGame();
    }
}