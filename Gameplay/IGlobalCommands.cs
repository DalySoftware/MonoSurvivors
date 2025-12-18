namespace Gameplay;

public interface IGlobalCommands
{
    void ShowGameOver();
    void ReturnToTitle();
    void OnLevelUp(int levelsGained);
    void ShowSphereGrid();
    void ShowPauseMenu();
    void ResumeGame();
    void Exit();
    void StartGame();
    void CloseSphereGrid();
}