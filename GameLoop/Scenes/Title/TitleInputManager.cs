using GameLoop.Input;
using Gameplay;

namespace GameLoop.Scenes.Title;

internal class TitleInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState,
    SceneManager sceneManager)
    : SingleActionSceneInputManager(globalCommands, inputState, sceneManager, globalCommands.StartGame);