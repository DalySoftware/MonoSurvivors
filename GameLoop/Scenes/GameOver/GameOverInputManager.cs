using GameLoop.Input;
using Gameplay;

namespace GameLoop.Scenes.GameOver;

internal class GameOverInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState,
    SceneManager sceneManager)
    : SingleActionSceneInputManager(globalCommands, inputState, sceneManager, globalCommands.StartGame);