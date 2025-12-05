using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Behaviour;
using Gameplay.Combat;
using Gameplay.Entities.Enemies;
using Gameplay.Levelling;
using Gameplay.Rendering;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Entities;

public class EntityManager(ContentManager content)
{
    private readonly List<IEntity> _entities = [];
    private readonly List<IEntity> _entitiesToAdd = [];
    private readonly EntityRenderer _renderer = new(content);

    private IEnumerable<EnemyBase> Enemies => _entities.OfType<EnemyBase>();

    public void Add(params IEnumerable<IEntity> entities) => _entitiesToAdd.AddRange(entities);
    public void Add(Func<IEntity> entityFactory) => _entitiesToAdd.Add(entityFactory());

    public void Update(GameTime gameTime)
    {
        foreach (var entity in _entities.ToList())
            entity.Update(gameTime);
        DamageProcessor.ApplyDamage(_entities);
        PickupProcessor.ProcessPickups(_entities);
        RemoveEntities();
        AddPendingEntities();
    }

    private void RemoveEntities() => _entities.RemoveAll(e => e.MarkedForDeletion);

    private void AddPendingEntities()
    {
        _entities.AddRange(_entitiesToAdd);
        _entitiesToAdd.Clear();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var entity in _entities.OfType<IHasPosition>()) _renderer.Draw(spriteBatch, entity);
    }

    internal EnemyBase? NearestEnemyTo(PlayerCharacter player) =>
        Enemies.MinBy(e => Vector2.DistanceSquared(player.Position, e.Position));
}