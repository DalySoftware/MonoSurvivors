using GameLoop.Input;
using GameLoop.Persistence;
using GameLoop.Rendering;
using GameLoop.Scenes.Gameplay.UI;
using GameLoop.UserSettings;
using Gameplay;
using Gameplay.Entities;
using Gameplay.Utilities;
using Microsoft.Xna.Framework.Input;
#if DEBUG
using GameLoop.Debug;
#endif

namespace GameLoop.Scenes.Gameplay;

internal class GameplayInputManager(
    PlayerCharacter player,
    IGlobalCommands globalCommands,
    GameInputState inputState,
    KeyBindingsSettings keyBindingsSettings,
    ISettingsPersistence persistence,
    IDisplayModeManager displayMode,
    PerformanceHud performanceHud)
{
#if !DEBUG
    private readonly ISettingsPersistence _ = persistence; // prevent "unused parameter" warning
#endif
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

        if (_actions.WasPressed(GameplayAction.ToggleFullscreen))
            displayMode.ToggleFullscreen();

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
        if (inputState.KeyboardState.IsKeyDown(Keys.LeftAlt) &&
            inputState.KeyboardState.IsKeyDown(Keys.P) &&
            inputState.PreviousKeyboardState.IsKeyUp(Keys.P))
            performanceHud.Toggle();
    }
}