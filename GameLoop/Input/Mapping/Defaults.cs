using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input.Mapping;

public static class Defaults
{
    public static ActionKeyMap<GameplayAction> Gameplay { get; } = new()
    {
        Gamepad = new Dictionary<GameplayAction, List<Buttons>>
        {
            [GameplayAction.MoveDown] = [Buttons.DPadDown],
            [GameplayAction.MoveLeft] = [Buttons.DPadLeft],
            [GameplayAction.MoveRight] = [Buttons.DPadRight],
            [GameplayAction.MoveUp] = [Buttons.DPadUp],
            [GameplayAction.OpenSphereGrid] = [Buttons.Back],
            [GameplayAction.Pause] = [Buttons.Start],
        },
        Keyboard = new Dictionary<GameplayAction, List<Keys>>
        {
            [GameplayAction.MoveUp] = [Keys.W, Keys.Up],
            [GameplayAction.MoveLeft] = [Keys.A, Keys.Left],
            [GameplayAction.MoveDown] = [Keys.S, Keys.Down],
            [GameplayAction.MoveRight] = [Keys.D, Keys.Right],
            [GameplayAction.OpenSphereGrid] = [Keys.Space, Keys.Tab],
            [GameplayAction.Pause] = [Keys.Escape],
        },
    };

    public static ActionKeyMap<PauseAction> PauseMenu { get; } = new()
    {
        Gamepad = new Dictionary<PauseAction, List<Buttons>>
        {
            [PauseAction.NavigateUp] = [Buttons.DPadUp],
            [PauseAction.NavigateDown] = [Buttons.DPadDown],
            [PauseAction.NavigateLeft] = [Buttons.DPadLeft],
            [PauseAction.NavigateRight] = [Buttons.DPadRight],
            [PauseAction.Activate] = [Buttons.A],
            [PauseAction.Resume] = [Buttons.Start, Buttons.B],
        },
        Keyboard = new Dictionary<PauseAction, List<Keys>>
        {
            [PauseAction.Resume] = [Keys.Escape],
        },
    };
}