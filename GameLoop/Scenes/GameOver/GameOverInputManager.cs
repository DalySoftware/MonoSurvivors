using GameLoop.Input;
using Gameplay;

namespace GameLoop.Scenes.GameOver;

internal class GameOverInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState,
    SceneManager sceneManager)
    : BaseInputManager(globalCommands, inputState, sceneManager)
{
    private readonly GameOverActionInput _actions = new(inputState);

    internal void Update()
    {
        if (ShouldSkipInput()) return;

        if (_actions.WasPressed(GameOverAction.Exit)) GlobalCommands.Exit();

        if (_actions.WasPressed(GameOverAction.StartGame)) GlobalCommands.StartGame();
    }
}