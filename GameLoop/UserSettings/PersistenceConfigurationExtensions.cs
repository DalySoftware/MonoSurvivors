using GameLoop.Persistence;
using Microsoft.Extensions.Configuration;

namespace GameLoop.UserSettings;

internal static class PersistenceConfigurationExtensions
{
    internal static IConfigurationBuilder AddPersistence(
        this IConfigurationBuilder builder,
        IPersistence persistence,
        string storageKey) =>
        builder.Add(new PersistenceConfigurationSource(persistence, storageKey));
}