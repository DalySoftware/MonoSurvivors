using GameLoop.Persistence;
using Microsoft.Extensions.Configuration;

namespace GameLoop.UserSettings;

internal sealed class PersistenceConfigurationSource(
    IPersistence persistence,
    string storageKey) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new PersistenceConfigurationProvider(persistence, storageKey);
}