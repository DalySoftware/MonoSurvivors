using System;
using GameLoop.UserSettings;
using Microsoft.Xna.Framework;

namespace GameLoop.Input;

public enum GameplayAction
{
    Pause,
    OpenSphereGrid,
    MoveLeft,
    MoveRight,
    MoveUp,
    MoveDown,
}

internal sealed class GameplayActionInput(GameInputState state, KeyBindingsSettings bindings)
    : ActionInputBase<GameplayAction>(state, bindings.GameplayActions)
{
    public Vector2 GetMovement()
    {
        var x = 0f;
        var y = 0f;

        if (IsDown(GameplayAction.MoveLeft)) x -= 1f;
        if (IsDown(GameplayAction.MoveRight)) x += 1f;
        if (IsDown(GameplayAction.MoveUp)) y -= 1f;
        if (IsDown(GameplayAction.MoveDown)) y += 1f;

        if (State.GamePadState.IsConnected)
        {
            var stick = State.GamePadState.ThumbSticks.Left;
            const float deadzone = 0.2f;

            if (MathF.Abs(stick.X) >= deadzone)
                x += stick.X;

            if (MathF.Abs(stick.Y) >= deadzone)
                y -= stick.Y;
        }

        return new Vector2(x, y);
    }
}