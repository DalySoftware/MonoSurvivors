using System;
using Gameplay.Audio;
using Gameplay.Utilities;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Entities.Enemies;

public class EnemySpawner(
    EntityManager entityManager,
    PlayerCharacter player,
    IAudioPlayer audio,
    GraphicsDevice graphics) : IEntity
{
    private readonly ExperienceSpawner _experienceSpawner = new(entityManager, player, audio);
    private readonly ScreenPositioner _screenPositioner = new(graphics);
    private TimeSpan _remainingCooldown = TimeSpan.Zero;
    public required TimeSpan SpawnDelay { get; set; }
    public int BatchSize { get; set; } = 1;

    public bool MarkedForDeletion => false;

    public void Update(GameTime gameTime)
    {
        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return;

        _remainingCooldown = SpawnDelay;
        for (var i = 0; i < BatchSize; i++)
            entityManager.Spawn(GetEnemyWithRandomPosition(BasicEnemy));

        entityManager.Spawn(GetEnemyWithRandomPosition(Hulker));
    }

    private BasicEnemy BasicEnemy(Vector2 position) => new(position, player)
    {
        OnDeath = OnDeath
    };

    private Hulker Hulker(Vector2 position) => new(position, player)
    {
        OnDeath = OnDeath
    };


    private EnemyBase GetEnemyWithRandomPosition(Func<Vector2, EnemyBase> factory) =>
        factory(_screenPositioner.GetRandomOffScreenPosition(player.Position));

    private void OnDeath(EnemyBase deadEnemy)
    {
        _experienceSpawner.SpawnExperienceFor(deadEnemy);
        audio.Play(SoundEffectTypes.EnemyDeath);
        player.TrackKills(1);
    }
}