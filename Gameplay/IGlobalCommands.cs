using Gameplay.Levelling.PowerUps;

namespace Gameplay;

public interface IGlobalCommands
{
    void ShowGameOver();
    void ReturnToTitle();
    void ShowSphereGrid();
    void ShowPauseMenu();
    void ResumeGame();
    void StartGame(WeaponDescriptor startingWeapon);
    void CloseSphereGrid();
    void ShowMouse();
    void HideMouse();
    void ShowWinGame();
}