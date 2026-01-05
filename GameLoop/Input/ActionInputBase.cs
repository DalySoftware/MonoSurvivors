using System;
using System.Linq;
using GameLoop.Input.Mapping;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

internal abstract class ActionInputBase<TAction>(GameInputState state, ActionKeyMap<TAction> map)
    where TAction : Enum
{
    protected readonly GameInputState State = state;

    internal bool WasPressed(TAction action) =>
        map.GetKeys(action).Any(WasPressed) ||
        map.GetButtons(action).Any(WasPressed);

    internal bool IsDown(TAction action) =>
        map.GetKeys(action).Any(k => State.KeyboardState.IsKeyDown(k)) ||
        map.GetButtons(action).Any(b => State.GamePadState.IsButtonDown(b));

    private bool WasPressed(Keys key) => State.KeyboardState.IsKeyDown(key) && State.PreviousKeyboardState.IsKeyUp(key);

    private bool WasPressed(Buttons button) =>
        State.GamePadState.IsButtonDown(button) && State.PreviousGamePadState.IsButtonUp(button);
}