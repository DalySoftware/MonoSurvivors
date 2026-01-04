using GameLoop.Input;
using Gameplay;

namespace GameLoop.Scenes.GameOver;

internal class GameOverInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState)
    : SingleActionSceneInputManager(globalCommands, inputState, globalCommands.StartGame);