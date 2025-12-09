using System;
using System.Collections.Generic;
using System.Linq;
using ContentLibrary;
using Gameplay.Audio;
using Gameplay.Combat;
using Gameplay.Combat.Weapons;
using Gameplay.Levelling.PowerUps;
using Gameplay.Levelling.PowerUps.Player;
using Gameplay.Levelling.PowerUps.Weapon;
using Gameplay.Rendering;
using Gameplay.Rendering.Effects;
using Gameplay.Utilities;

namespace Gameplay.Entities;

public class PlayerCharacter(Vector2 position, EffectManager effectManager, IAudioPlayer audio, Action? onDeath = null)
    : MovableEntity(position), IDamageablePlayer, IVisual
{
    private const float BaseSpeed = 0.3f;
    private float Speed => BaseSpeed * (1f + _powerUps.OfType<SpeedUp>().Sum(p => p.Value));
    private readonly TimeSpan _invincibilityOnHit = TimeSpan.FromSeconds(0.5);
    private TimeSpan _invincibilityDuration = TimeSpan.Zero;
    
    private readonly List<IPlayerPowerUp> _powerUps = [];

    public float PickupRadiusMultiplier => _powerUps.OfType<PickupRadiusUp>().Sum(p => p.Value) + 1f;
    public WeaponBelt WeaponBelt { get; } = new();

    public float Experience { get; private set; }
    public event EventHandler<PlayerCharacter> OnExperienceGain = (_, _) => { };

    public void GainExperience(float amount)
    {
        Experience += amount;
        OnExperienceGain(this, this);
    }

    private const int BaseHealth = 6;
    public int MaxHealth => BaseHealth + _powerUps.OfType<MaxHealthUp>().Sum(p => p.Value);
    public int Health { get; private set; } = BaseHealth;
    
    public bool Damageable => _invincibilityDuration <= TimeSpan.Zero;

    public float CollisionRadius => 16f;

    public void TakeDamage(int damage)
    {
        if (_invincibilityDuration > TimeSpan.Zero) return;

        Health -= damage;
        _invincibilityDuration = _invincibilityOnHit;
        effectManager.FireEffect(this, new GreyscaleEffect(_invincibilityOnHit));
        audio.Play(SoundEffectTypes.PlayerHurt);

        if (Health > 0) return;

        Health = 0;
        MarkedForDeletion = true;
        onDeath?.Invoke();
    }

    public string TexturePath => Paths.Images.Player;

    public void DirectionInput(UnitVector2 input) => Velocity = (Vector2)input * Speed;

    public override void Update(GameTime gameTime)
    {
        _invincibilityDuration -= gameTime.ElapsedGameTime;
        WeaponBelt.Update(gameTime);
        base.Update(gameTime);
    }
    
    internal void AddPowerUp(IPowerUp powerUp)
    {
        switch (powerUp)
        {
            case IWeaponPowerUp weaponPowerUp:
                WeaponBelt.AddPowerUp(weaponPowerUp);
                return;
            case IPlayerPowerUp playerPowerUp:
            {
                _powerUps.Add(playerPowerUp);
            
                // Extra effects
                if (playerPowerUp is MaxHealthUp maxHealthUp)
                    Health += maxHealthUp.Value;
                break;
            }
        }
    }
}