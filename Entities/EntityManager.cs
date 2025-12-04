using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ContentLibrary;
using Entities.Combat;
using Entities.Combat.Weapons.Projectile;
using Entities.Enemy;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Rendering;

namespace Entities;

public class EntityManager(ContentManager content)
{
    private readonly Texture2D _bulletTexture = content.Load<Texture2D>(Paths.Images.Bullet);
    private readonly Texture2D _enemyTexture = content.Load<Texture2D>(Paths.Images.Enemy);
    private readonly ConcurrentBag<IEntity> _entities = [];
    private readonly Texture2D _playerTexture = content.Load<Texture2D>(Paths.Images.Player);

    private IEnumerable<EnemyBase> Enemies => _entities.OfType<EnemyBase>();

    public void Add(IEntity entity) => _entities.Add(entity);
    public void Add(Func<IEntity> entityFactory) => _entities.Add(entityFactory());

    public void Update(GameTime gameTime)
    {
        foreach (var entity in _entities)
            entity.Update(gameTime);
        DamageProcessor.ApplyDamage(_entities);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var entity in _entities.OfType<IHasPosition>()) Draw(spriteBatch, entity);
    }

    private void Draw(SpriteBatch spriteBatch, IHasPosition entity)
    {
        var texture = Texture(entity);
        spriteBatch.Draw(texture, entity.Position, origin: texture.Centre);
    }

    private Texture2D Texture(IHasPosition entity) => entity switch
    {
        BasicEnemy => _enemyTexture,
        PlayerCharacter => _playerTexture,
        Bullet => _bulletTexture,
        _ => throw new Exception("Unmapped entity texture")
    };

    internal EnemyBase? NearestEnemyTo(PlayerCharacter player) =>
        Enemies.MinBy(e => Vector2.DistanceSquared(player.Position, e.Position));
}