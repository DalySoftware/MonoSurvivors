using GameLoop.Persistence;
using GameLoop.UserSettings;
using Gameplay.Entities;

namespace GameLoop.Debug;

internal static class DebugActions
{
    internal static void GrantExperience(PlayerCharacter player) => player.GainExperience(100f);

    internal static void SaveKeybinds(KeyBindingsSettings settings, ISettingsPersistence persistence) =>
        persistence.Save(settings, PersistenceJsonContext.Default.KeyBindingsSettings);
}