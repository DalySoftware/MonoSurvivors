using GameLoop.Input;
using GameLoop.Input.Mapping;

namespace GameLoop.UserSettings;

public sealed class KeyBindingsSettings : IMergeable<KeyBindingsSettings>
{
    public ActionKeyMap<GameplayAction> GameplayActions { get; set; } = Defaults.Gameplay;
    public ActionKeyMap<PauseAction> PauseMenuActions { get; set; } = Defaults.PauseMenu;

    public void MergeFrom(KeyBindingsSettings overrides)
    {
        GameplayActions.MergeFrom(overrides.GameplayActions);
        PauseMenuActions.MergeFrom(overrides.PauseMenuActions);
    }
}