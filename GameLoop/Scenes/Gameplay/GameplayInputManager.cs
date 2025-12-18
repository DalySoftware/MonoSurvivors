using GameLoop.Input;
using Gameplay;
using Gameplay.Entities;
using Gameplay.Utilities;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.Gameplay;

internal class GameplayInputManager(PlayerCharacter player, IGlobalCommands globalCommands) : BaseInputManager
{
    internal override void Update()
    {
        base.Update();

        if (WasPressedThisFrame(Keys.Escape) || GamePadState.Buttons.Start == ButtonState.Pressed)
        {
            globalCommands.ShowPauseMenu();
            return;
        }

        if (WasPressedThisFrame(Keys.Tab))
        {
            globalCommands.ShowSphereGrid();
            return;
        }

        var x = 0f;
        var y = 0f;

        // Keyboard input
        if (KeyboardState.IsKeyDown(Keys.S)) x -= 1f;
        if (KeyboardState.IsKeyDown(Keys.F)) x += 1f;
        if (KeyboardState.IsKeyDown(Keys.E)) y -= 1f;
        if (KeyboardState.IsKeyDown(Keys.D)) y += 1f;

        // GamePad input (left thumbstick)
        if (GamePadState.IsConnected)
        {
            var thumbStick = GamePadState.ThumbSticks.Left;
            x += thumbStick.X;
            y -= thumbStick.Y; // Y is inverted on thumbsticks
        }

        if (x != 0f || y != 0f)
            player.DirectionInput(new UnitVector2(x, y));
        else
            player.DirectionInput(new UnitVector2(0f, 0f));

#if DEBUG
        if (KeyboardState.IsKeyDown(Keys.OemPlus) && KeyboardState.IsKeyDown(Keys.LeftControl))
            player.GainExperience(100f);
#endif
    }
}