using GameLoop.Input;
using Gameplay;

namespace GameLoop.Scenes.Title;

internal class TitleInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState,
    SceneManager sceneManager)
    : BaseInputManager(globalCommands, inputState, sceneManager)
{
    private readonly TitleActionInput _actions = new(inputState);

    internal void Update()
    {
        if (ShouldSkipInput()) return;

        if (_actions.WasPressed(TitleAction.Exit)) GlobalCommands.Exit();

        if (_actions.WasPressed(TitleAction.StartGame)) GlobalCommands.StartGame();
    }
}