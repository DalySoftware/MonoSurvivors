using System;
using GameLoop.Scenes;
using Gameplay;

namespace GameLoop.Input;

internal class SingleActionSceneInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState,
    SceneManager sceneManager,
    Action primaryAction)
    : BaseInputManager(globalCommands, inputState, sceneManager)
{
    private readonly SingleActionSceneActionInput _actions = new(inputState);

    internal void Update()
    {
        if (ShouldSkipInput()) return;

        if (_actions.WasPressed(SingleActionSceneAction.Exit)) GlobalCommands.Exit();

        if (_actions.WasPressed(SingleActionSceneAction.PrimaryAction)) primaryAction();
    }
}