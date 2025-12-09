using System;
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
    private const float BaseSpeed = 0.25f;
    private float _speedMultiplier = 1f;
    private float Speed => BaseSpeed * _speedMultiplier;
    
    private readonly TimeSpan _invincibilityOnHit = TimeSpan.FromSeconds(0.5);
    private TimeSpan _invincibilityDuration = TimeSpan.Zero;
    
    public float PickupRadiusMultiplier { get; private set; } = 1f;
    public WeaponBelt WeaponBelt { get; } = new();

    public float Experience { get; private set; }
    public event EventHandler<PlayerCharacter> OnExperienceGain = (_, _) => { };

    public void GainExperience(float amount)
    {
        Experience += amount;
        OnExperienceGain(this, this);
    }

    private const int BaseHealth = 6;
    public int MaxHealth { get; private set; } = BaseHealth;
    public int Health { get; private set; } = BaseHealth;
    
    public bool Damageable => _invincibilityDuration <= TimeSpan.Zero;

    public float CollisionRadius => 32f;

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
            case PickupRadiusUp radiusUp:
                PickupRadiusMultiplier += radiusUp.Value;
                break;
            case SpeedUp speedUp:
                _speedMultiplier += speedUp.Value;
                break;
            case MaxHealthUp maxHealthUp:
                MaxHealth += maxHealthUp.Value;
                Health += maxHealthUp.Value;
                break;
            case IWeaponPowerUp weaponPowerUp:
                WeaponBelt.AddPowerUp(weaponPowerUp);
                return;
        }
    }
}