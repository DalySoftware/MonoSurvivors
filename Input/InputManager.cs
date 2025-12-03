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

        if (keyboardState.IsKeyDown(Keys.Escape))
        {
            OnExit();
            return;
        }

        var x = 0f;
        var y = 0f;

        if (keyboardState.IsKeyDown(Keys.S)) x -= 1f;
        if (keyboardState.IsKeyDown(Keys.F)) x += 1f;
        if (keyboardState.IsKeyDown(Keys.E)) y -= 1f;
        if (keyboardState.IsKeyDown(Keys.D)) y += 1f;

        if (x != 0f || y != 0f)
            player.DirectionInput(new UnitVector2(x, y));
        else
            player.DirectionInput(new UnitVector2(0f, 0f));
    }
}