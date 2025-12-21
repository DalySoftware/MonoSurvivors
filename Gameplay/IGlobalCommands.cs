namespace Gameplay;

public interface IGlobalCommands
{
    void ShowGameOver();
    void ReturnToTitle();
    void ShowSphereGrid();
    void ShowPauseMenu();
    void ResumeGame();
    void Exit();
    void StartGame();
    void CloseSphereGrid();
    void ShowMouse();
    void HideMouse();
}