using GameLoop.Input;
using Gameplay;
using Gameplay.Entities;
using Gameplay.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.Gameplay;

internal class GameplayInputManager(
    PlayerCharacter player,
    IGlobalCommands globalCommands,
    GameInputState inputState,
    SceneManager sceneManager)
    : BaseInputManager(globalCommands, inputState, sceneManager)
{
    internal void Update(GameTime gameTime)
    {
        if (ShouldSkipInput()) return;

        if (WasPressedThisFrame(Keys.Escape) || WasPressedThisFrame(Buttons.Start))
        {
            GlobalCommands.ShowPauseMenu();
            return;
        }

        if (WasPressedThisFrame(Keys.Space) || WasPressedThisFrame(Keys.Tab) || WasPressedThisFrame(Buttons.Back))
        {
            GlobalCommands.ShowSphereGrid();
            return;
        }

        var x = 0f;
        var y = 0f;

        // Keyboard input
        if (InputState.KeyboardState.IsKeyDown(Keys.S)) x -= 1f;
        if (InputState.KeyboardState.IsKeyDown(Keys.F)) x += 1f;
        if (InputState.KeyboardState.IsKeyDown(Keys.E)) y -= 1f;
        if (InputState.KeyboardState.IsKeyDown(Keys.D)) y += 1f;

        // GamePad input (left thumbstick)
        if (InputState.GamePadState.IsConnected)
        {
            var thumbStick = InputState.GamePadState.ThumbSticks.Left;
            x += thumbStick.X;
            y -= thumbStick.Y; // Y is inverted on thumbsticks
        }

        if (x != 0f || y != 0f)
            player.DirectionInput(new UnitVector2(x, y));
        else
            player.DirectionInput(new UnitVector2(0f, 0f));

#if DEBUG
        if (InputState.KeyboardState.IsKeyDown(Keys.LeftControl) && WasPressedThisFrame(Keys.OemPlus))
            player.GainExperience(100f);
#endif
    }
}