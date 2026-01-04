using System;
using Gameplay;

namespace GameLoop.Input;

internal class SingleActionSceneInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState,
    Action primaryAction)
{
    private readonly SingleActionSceneActionInput _actions = new(inputState);

    internal void Update()
    {
        if (_actions.WasPressed(SingleActionSceneAction.Exit)) globalCommands.Exit();

        if (_actions.WasPressed(SingleActionSceneAction.PrimaryAction)) primaryAction();
    }
}