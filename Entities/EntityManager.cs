using System;
using System.Collections.Generic;
using System.Linq;
using ContentLibrary;
using Entities.Enemy;
using Entities.Weapons.Projectile;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Rendering;

namespace Entities;

public class EntityManager(ContentManager content)
{
    private readonly List<Bullet> _bullets = [];
    private readonly Texture2D _bulletTexture = content.Load<Texture2D>(Paths.Images.Bullet);
    private readonly List<EnemyBase> _enemies = [];
    private readonly Texture2D _enemyTexture = content.Load<Texture2D>(Paths.Images.Enemy);
    private readonly List<PlayerCharacter> _players = [];
    private readonly Texture2D _playerTexture = content.Load<Texture2D>(Paths.Images.Player);
    private readonly List<BasicGun> _weapons = [];

    private IEnumerable<IEntity> Entities => [.._players, .._enemies, .._bullets, .._weapons];

    public void Add(BasicGun weapon) => _weapons.Add(weapon);

    public void Add(Bullet bullet) => _bullets.Add(bullet);

    public void Add(Func<EnemyBase> enemyFactory) => _enemies.Add(enemyFactory());

    public void Add(PlayerCharacter character) => _players.Add(character);

    public void Update(GameTime gameTime)
    {
        foreach (var entity in Entities)
            entity.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var entity in Entities.OfType<IHasPosition>()) Draw(spriteBatch, entity);
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
        _enemies.MinBy(e => Vector2.DistanceSquared(player.Position, e.Position));
}