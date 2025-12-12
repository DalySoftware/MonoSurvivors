using System;
using ContentLibrary;
using Gameplay.Audio;
using Gameplay.CollisionDetection;
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
    : MovableEntity(position), IDamageablePlayer, ISimpleVisual
{
    private const float BaseSpeed = 0.25f;

    private const int BaseHealth = 6;

    private readonly TimeSpan _invincibilityOnHit = TimeSpan.FromSeconds(0.5);
    private float _experienceMultiplier = 1f;
    private TimeSpan _invincibilityDuration = TimeSpan.Zero;

    private int _killsSinceLastLifeSteal = 0;
    private int _lifeSteal = 0;
    private float _speedMultiplier = 1f;

    private float Speed => BaseSpeed * _speedMultiplier;

    public float PickupRadiusMultiplier { get; private set; } = 1f;
    public WeaponBelt WeaponBelt { get; } = new();

    public float Experience { get; private set; }
    public int MaxHealth { get; private set; } = BaseHealth;

    private int KillsPerHeal => _lifeSteal == 0 ? int.MaxValue : 100 / _lifeSteal;

    public ICollider Collider => new CircleCollider(this, 32f);

    public int Health
    {
        get;
        private set => field = Math.Clamp(value, 0, MaxHealth);
    } = BaseHealth;

    public bool Damageable => _invincibilityDuration <= TimeSpan.Zero;

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
    public event EventHandler<PlayerCharacter> OnExperienceGain = (_, _) => { };

    public void GainExperience(float amount)
    {
        Experience += amount * _experienceMultiplier;
        OnExperienceGain(this, this);
    }

    public void DirectionInput(UnitVector2 input) => Velocity = (Vector2)input * Speed;

    public override void Update(GameTime gameTime)
    {
        _invincibilityDuration -= gameTime.ElapsedGameTime;
        WeaponBelt.Update(gameTime);
        ApplyLifeSteal();
        base.Update(gameTime);
    }

    public void TrackKills(int numberOfKills) => _killsSinceLastLifeSteal += numberOfKills;

    private void ApplyLifeSteal()
    {
        if (_killsSinceLastLifeSteal < KillsPerHeal) return;

        Health += 1;
        _killsSinceLastLifeSteal -= KillsPerHeal;
    }

    public void AddPowerUp(IPowerUp powerUp)
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
            case LifeStealUp lifeStealUp:
                _lifeSteal += lifeStealUp.Value;
                break;
            case ExperienceUp experienceUp:
                _experienceMultiplier += experienceUp.Value;
                break;
            case IWeaponPowerUp weaponPowerUp:
                WeaponBelt.AddPowerUp(weaponPowerUp);
                return;
        }
    }
}