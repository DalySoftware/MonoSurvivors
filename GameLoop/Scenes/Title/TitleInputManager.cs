using GameLoop.Input;
using Gameplay;

namespace GameLoop.Scenes.Title;

internal class TitleInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState)
    : SingleActionSceneInputManager(globalCommands, inputState, globalCommands.StartGame);