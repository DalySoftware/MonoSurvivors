using GameLoop.Input;
using GameLoop.Persistence;
using Gameplay;

namespace GameLoop.Scenes.GameOver;

internal class GameOverInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState,
    ISettingsPersistence settingsPersistence)
    : SingleActionSceneInputManager(globalCommands, inputState, settingsPersistence, globalCommands.ReturnToTitle);