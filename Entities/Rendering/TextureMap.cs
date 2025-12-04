using System;
using ContentLibrary;
using Entities.Combat.Weapons.Projectile;
using Entities.Enemy;
using Entities.Levelling;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Entities.Rendering;

internal class TextureMap(ContentManager content)
{
    private readonly Texture2D _bulletTexture = content.Load<Texture2D>(Paths.Images.Bullet);
    private readonly Texture2D _enemyTexture = content.Load<Texture2D>(Paths.Images.Enemy);
    private readonly Texture2D _experienceTexture = content.Load<Texture2D>(Paths.Images.Experience);
    private readonly Texture2D _playerTexture = content.Load<Texture2D>(Paths.Images.Player);

    internal Texture2D TextureFor(IHasPosition entity) => entity switch
    {
        BasicEnemy => _enemyTexture,
        PlayerCharacter => _playerTexture,
        Bullet => _bulletTexture,
        Experience => _experienceTexture,
        _ => throw new Exception("Unmapped entity texture")
    };
}