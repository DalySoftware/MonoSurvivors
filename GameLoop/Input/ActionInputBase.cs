using System;
using System.Collections.Generic;
using GameLoop.Input.Mapping;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

internal abstract class ActionInputBase<TAction>(GameInputState state, ActionKeyMap<TAction> map)
    where TAction : Enum
{
    protected readonly GameInputState State = state;

    internal bool WasPressed(TAction action) =>
        AnyPressed(map.GetKeys(action)) ||
        AnyPressed(map.GetButtons(action));

    internal bool IsDown(TAction action) =>
        AnyDown(map.GetKeys(action)) ||
        AnyDown(map.GetButtons(action));

    private bool AnyPressed(IReadOnlyList<Keys> keys)
    {
        foreach (var key in keys)
            if (State.KeyboardState.IsKeyDown(key) && State.PreviousKeyboardState.IsKeyUp(key))
                return true;
        return false;
    }

    private bool AnyPressed(IReadOnlyList<Buttons> buttons)
    {
        foreach (var button in buttons)
            if (State.GamePadState.IsButtonDown(button) && State.PreviousGamePadState.IsButtonUp(button))
                return true;
        return false;
    }

    private bool AnyDown(IReadOnlyList<Keys> keys)
    {
        var ks = State.KeyboardState;
        foreach (var key in keys)
            if (ks.IsKeyDown(key))
                return true;
        return false;
    }

    private bool AnyDown(IReadOnlyList<Buttons> buttons)
    {
        var gs = State.GamePadState;
        foreach (var button in buttons)
            if (gs.IsButtonDown(button))
                return true;
        return false;
    }
}