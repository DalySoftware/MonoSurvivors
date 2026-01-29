using System;

namespace Gameplay.Entities.Enemies;

public sealed class ApplyVectorEvery(Func<Vector2> source, Action<Vector2> apply, TimeSpan interval) : IEnemyBehaviour
{
    private TimeSpan _cooldown;

    public void BeforeMove(GameTime gameTime)
    {
        if (_cooldown <= TimeSpan.Zero)
        {
            apply(source());
            _cooldown = interval;
        }

        _cooldown -= gameTime.ElapsedGameTime;
    }

    public void AfterMove(GameTime gameTime) { }
}