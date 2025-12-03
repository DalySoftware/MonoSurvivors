using System;
using Characters;
using Characters.Enemy;
using Microsoft.Xna.Framework;

namespace GameLoop;

internal class EnemySpawner
{
    private readonly Random _random = new();

    internal BasicEnemy GetEnemyWithRandomPosition(PlayerCharacter player)
    {
        const float distanceFromPlayer = 500f;
        var angle = _random.NextDouble() * 2 * Math.PI;

        var x = player.Position.X + (float)Math.Cos(angle) * distanceFromPlayer;
        var y = player.Position.Y + (float)Math.Sin(angle) * distanceFromPlayer;

        var position = new Vector2(x, y);

        return new BasicEnemy(position, player);
    }
}