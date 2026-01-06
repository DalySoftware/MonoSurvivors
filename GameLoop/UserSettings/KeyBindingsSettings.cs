using GameLoop.Input;
using GameLoop.Input.Mapping;

namespace GameLoop.UserSettings;

public sealed class KeyBindingsSettings
{
    internal ActionKeyMap<GameplayAction> GameplayActions { get; set; } = Defaults.Gameplay;
    internal ActionKeyMap<PauseAction> PauseMenuActions { get; set; } = Defaults.PauseMenu;
    internal ActionKeyMap<SphereGridAction> SphereGridActions { get; set; } = Defaults.SphereGrid;
    internal ActionKeyMap<SingleActionSceneAction> SingleActionScenes { get; set; } = Defaults.SingleActionScenes;
}