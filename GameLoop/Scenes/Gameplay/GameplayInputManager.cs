using GameLoop.Debug;
using GameLoop.Input;
using GameLoop.Persistence;
using GameLoop.UserSettings;
using Gameplay;
using Gameplay.Entities;
using Gameplay.Utilities;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.Gameplay;

internal class GameplayInputManager(
    PlayerCharacter player,
    IGlobalCommands globalCommands,
    GameInputState inputState,
    KeyBindingsSettings keyBindingsSettings,
    ISettingsPersistence persistence)
{
    private readonly GameplayActionInput _actions = new(inputState, keyBindingsSettings);

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
            inputState.PreviousKeyboardState.IsKeyUp(Keys.OemPlus))
            DebugActions.GrantExperience(player);

        if (inputState.KeyboardState.IsKeyDown(Keys.LeftControl) &&
            inputState.KeyboardState.IsKeyDown(Keys.S))
            DebugActions.SaveKeybinds(keyBindingsSettings, persistence);
#endif
    }
}