using GameLoop.Scenes;
using Gameplay;

namespace GameLoop.Input;

/// <summary>
///     Contains any global input logic, eg exit
/// </summary>
internal abstract class BaseInputManager(
    IGlobalCommands commands,
    GameInputState inputState,
    SceneManager sceneManager)
{
    protected readonly GameInputState InputState = inputState;
    protected readonly IGlobalCommands GlobalCommands = commands;

    protected bool ShouldSkipInput()
    {
        if (sceneManager.InputFramesToSkip <= 0) return false;

        sceneManager.InputFramesToSkip--;
        return true;
    }
}