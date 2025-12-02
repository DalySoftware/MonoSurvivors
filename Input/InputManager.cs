using Characters;
using Characters.Utilities;
using Microsoft.Xna.Framework.Input;

namespace Input;

public class InputManager(PlayerCharacter player)
{
    public void Update()
    {
        var keyboardState = Keyboard.GetState();

        float x = 0f;
        float y = 0f;

        if (keyboardState.IsKeyDown(Keys.S)) x -= 1f;
        if (keyboardState.IsKeyDown(Keys.F)) x += 1f;
        if (keyboardState.IsKeyDown(Keys.E)) y -= 1f;
        if (keyboardState.IsKeyDown(Keys.D)) y += 1f;

        if (x != 0f || y != 0f)
        {
            player.DirectionInput(new UnitVector2(x, y));
        }
        else
        {
            player.DirectionInput(new UnitVector2(0f, 0f));
        }
    }
}