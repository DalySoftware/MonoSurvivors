using GameLoop.Input;
using GameLoop.Persistence;
using Gameplay;

namespace GameLoop.Scenes.Title;

internal class TitleInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState,
    ISettingsPersistence settingsPersistence)
    : SingleActionSceneInputManager(globalCommands, inputState, settingsPersistence, globalCommands.StartGame);