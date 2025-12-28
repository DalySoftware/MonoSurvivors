using GameLoop.Scenes;
using Gameplay;
using Microsoft.Xna.Framework.Input;

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

    protected InputMethod CurrentInputMethod => InputState.CurrentInputMethod;

    protected bool ShouldSkipInput()
    {
        if (sceneManager.InputFramesToSkip <= 0) return false;

        sceneManager.InputFramesToSkip--;
        return true;
    }

    protected bool WasPressedThisFrame(Keys key) =>
        InputState.KeyboardState.IsKeyDown(key) && InputState.PreviousKeyboardState.IsKeyUp(key);

    protected bool WasPressedThisFrame(Buttons button) =>
        InputState.GamePadState.IsButtonDown(button) && InputState.PreviousGamePadState.IsButtonUp(button);
}