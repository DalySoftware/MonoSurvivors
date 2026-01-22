using GameLoop.Input;
using GameLoop.Persistence;
using Gameplay;

namespace GameLoop.Scenes.GameOver;

internal class GameOverInputManager(
    IGlobalCommands globalCommands,
    IAppLifeCycle appLifeCycle,
    GameInputState inputState,
    ISettingsPersistence settingsPersistence)
    : SingleActionSceneInputManager(appLifeCycle, inputState, settingsPersistence, globalCommands.ReturnToTitle);