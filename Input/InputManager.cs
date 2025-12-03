using System;
using Characters;
using Characters.Utilities;
using Microsoft.Xna.Framework.Input;

namespace Input;

public class InputManager(PlayerCharacter player)
{
    public Action OnExit { get; init; } = () => { };

    public void Update()
    {
        var keyboardState = Keyboard.GetState();
        var gamePadState = GamePad.GetState(0);

        if (keyboardState.IsKeyDown(Keys.Escape) || gamePadState.Buttons.Back == ButtonState.Pressed)
        {
            OnExit();
            return;
        }

        var x = 0f;
        var y = 0f;

        // Keyboard input
        if (keyboardState.IsKeyDown(Keys.S)) x -= 1f;
        if (keyboardState.IsKeyDown(Keys.F)) x += 1f;
        if (keyboardState.IsKeyDown(Keys.E)) y -= 1f;
        if (keyboardState.IsKeyDown(Keys.D)) y += 1f;

        // GamePad input (left thumbstick)
        if (gamePadState.IsConnected)
        {
            var thumbStick = gamePadState.ThumbSticks.Left;
            x += thumbStick.X;
            y -= thumbStick.Y; // Y is inverted on thumbsticks
        }

        if (x != 0f || y != 0f)
            player.DirectionInput(new UnitVector2(x, y));
        else
            player.DirectionInput(new UnitVector2(0f, 0f));
    }
}