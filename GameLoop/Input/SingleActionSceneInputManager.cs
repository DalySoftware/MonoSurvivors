using System;
using GameLoop.Persistence;
using Gameplay;

namespace GameLoop.Input;

internal class SingleActionSceneInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState,
    ISettingsPersistence settingsPersistence,
    Action primaryAction)
{
    private readonly SingleActionSceneActionInput _actions =
        new(inputState,
            settingsPersistence.Load(PersistenceJsonContext.Default.KeyBindingsSettings).SingleActionScenes);

    internal void Update()
    {
        if (_actions.WasPressed(SingleActionSceneAction.Exit)) globalCommands.Exit();

        if (_actions.WasPressed(SingleActionSceneAction.PrimaryAction)) primaryAction();
    }
}