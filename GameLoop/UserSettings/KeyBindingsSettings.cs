using GameLoop.Input;
using GameLoop.Input.Mapping;

namespace GameLoop.UserSettings;

public sealed class KeyBindingsSettings : IMergeable<KeyBindingsSettings>
{
    internal ActionKeyMap<GameplayAction> GameplayActions { get; set; } = Defaults.Gameplay;
    internal ActionKeyMap<PauseAction> PauseMenuActions { get; set; } = Defaults.PauseMenu;
    internal ActionKeyMap<SphereGridAction> SphereGridActions { get; set; } = Defaults.SphereGrid;
    internal ActionKeyMap<SingleActionSceneAction> SingleActionScenes { get; set; } = Defaults.SingleActionScenes;

    public void MergeFrom(KeyBindingsSettings overrides)
    {
        GameplayActions.MergeFrom(overrides.GameplayActions);
        PauseMenuActions.MergeFrom(overrides.PauseMenuActions);
        SphereGridActions.MergeFrom(overrides.SphereGridActions);
        SingleActionScenes.MergeFrom(overrides.SingleActionScenes);
    }
}