using GameLoop.Scenes;

namespace GameLoop.Input;

internal sealed class InputGate(SceneManager sceneManager)
{
    public bool ShouldProcessInput()
    {
        if (sceneManager.InputFramesToSkip <= 0)
            return true;

        sceneManager.InputFramesToSkip--;
        return false;
    }
}