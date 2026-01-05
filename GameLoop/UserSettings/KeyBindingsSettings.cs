using GameLoop.Input;
using GameLoop.Input.Mapping;

namespace GameLoop.UserSettings;

public sealed class KeyBindingsSettings : IMergeable<KeyBindingsSettings>
{
    internal ActionKeyMap<GameplayAction> GameplayActions { get; } = Defaults.Gameplay;
    internal ActionKeyMap<PauseAction> PauseMenuActions { get; } = Defaults.PauseMenu;
    internal ActionKeyMap<SphereGridAction> SphereGridActions { get; } = Defaults.SphereGrid;
    internal ActionKeyMap<SingleActionSceneAction> SingleActionScenes { get; } = Defaults.SingleActionScenes;

    public void MergeFrom(KeyBindingsSettings overrides)
    {
        GameplayActions.MergeFrom(overrides.GameplayActions);
        PauseMenuActions.MergeFrom(overrides.PauseMenuActions);
    }
}