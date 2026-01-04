using GameLoop.Input;
using Gameplay;
using Gameplay.Entities;
using Gameplay.Utilities;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.Gameplay;

internal class GameplayInputManager(
    PlayerCharacter player,
    IGlobalCommands globalCommands,
    GameInputState inputState)
{
    private readonly GameplayActionInput _actions = new(inputState);

    internal void Update()
    {
        if (_actions.WasPressed(GameplayAction.Pause))
        {
            globalCommands.ShowPauseMenu();
            return;
        }

        if (_actions.WasPressed(GameplayAction.OpenSphereGrid))
        {
            globalCommands.ShowSphereGrid();
            return;
        }

        var movement = _actions.GetMovement();
        player.DirectionInput(new UnitVector2(movement.X, movement.Y));

#if DEBUG
        if (inputState.KeyboardState.IsKeyDown(Keys.LeftControl) &&
            inputState.KeyboardState.IsKeyDown(Keys.OemPlus) &&
            inputState.PreviousKeyboardState.IsKeyDown(Keys.OemPlus))
            player.GainExperience(100f);
#endif
    }
}