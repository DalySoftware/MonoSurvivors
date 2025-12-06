using System;
using ContentLibrary;
using Gameplay.Combat;
using Gameplay.Rendering;
using Gameplay.Utilities;

namespace Gameplay.Entities;

public class PlayerCharacter(Vector2 position) : MovableEntity(position), IDamageablePlayer, IVisual
{
    private const float Speed = 0.5f;
    private readonly TimeSpan _invincibilityOnHit = TimeSpan.FromSeconds(0.5);

    private TimeSpan _invincibilityDuration = TimeSpan.Zero;

    public float Experience { get; set; } = 0f;
    public int MaxHealth => 6;
    public bool Damageable => _invincibilityDuration <= TimeSpan.Zero;

    public int Health { get; private set; } = 6;

    public float CollisionRadius => 16f;

    public void TakeDamage(int damage)
    {
        if (_invincibilityDuration > TimeSpan.Zero) return;

        Health -= damage;
        _invincibilityDuration = _invincibilityOnHit;

        if (Health > 0) return;

        Health = 0;
        MarkedForDeletion = true;
    }

    public string TexturePath => Paths.Images.Player;

    public void DirectionInput(UnitVector2 input) => Velocity = (Vector2)input * Speed;

    public override void Update(GameTime gameTime)
    {
        _invincibilityDuration -= gameTime.ElapsedGameTime;
        base.Update(gameTime);
    }
}