using System;
using ContentLibrary;
using Gameplay.Audio;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Combat;
using Gameplay.Combat.Weapons;
using Gameplay.Entities.Enemies;
using Gameplay.Levelling.PowerUps;
using Gameplay.Levelling.PowerUps.Player;
using Gameplay.Levelling.PowerUps.Weapon;
using Gameplay.Rendering;
using Gameplay.Rendering.Effects.SpriteBatch;
using Gameplay.Stats;
using Gameplay.Utilities;

namespace Gameplay.Entities;

public class PlayerCharacter : MovableEntity, IDamageablePlayer, ISpriteVisual
{
    private readonly TimeSpan _invincibilityOnHit = TimeSpan.FromSeconds(0.5);
    private readonly Action? _onDeath;

    private TimeSpan _invincibilityDuration = TimeSpan.Zero;
    private int _killsSinceLastLifeSteal = 0;
    private readonly EffectManager _effectManager;
    private readonly IAudioPlayer _audio;
    private readonly HealthRegenManager _healthRegen;
    private readonly EnemyDeathBlast _deathBlast;
    private readonly StatsCounter _runStats;

    public PlayerCharacter(
        Vector2 position,
        EffectManager effectManager,
        IAudioPlayer audio,
        IGlobalCommands globalCommands,
        WeaponBelt weaponBelt,
        HealthRegenManager healthRegen,
        PlayerStats stats,
        WeaponFactory weaponFactory,
        EnemyDeathBlast deathBlast,
        StatsCounter runStats) : base(position)
    {
        _effectManager = effectManager;
        _audio = audio;
        _healthRegen = healthRegen;
        _deathBlast = deathBlast;
        _runStats = runStats;
        _onDeath = globalCommands.ShowGameOver;
        WeaponFactory = weaponFactory;
        WeaponBelt = weaponBelt;
        Stats = stats;

        Colliders = [new CircleCollider(this, 32f)];
    }

    public WeaponFactory WeaponFactory { get; }
    public WeaponBelt WeaponBelt { get; }

    public float Experience { get; private set; }

    public PlayerStats Stats { get; }

    public int Health
    {
        get;
        private set => field = Math.Clamp(value, 0, Stats.MaxHealth);
    } = PlayerStats.BaseHealth;

    public ICollider[] Colliders { get; }

    public bool Damageable => _invincibilityDuration <= TimeSpan.Zero;
    public float Layer => Layers.Player;

    public string TexturePath => Paths.Images.Player;

    public void TakeDamage(GameTime gameTime, int damage)
    {
        if (_invincibilityDuration > TimeSpan.Zero) return;

        if (Random.Shared.NextSingle() < Stats.DodgeChance) return;

        Health -= damage;
        _invincibilityDuration = _invincibilityOnHit;
        _effectManager.FireEffect(this, SpriteBatchEffect.Greyscale, gameTime, _invincibilityOnHit);
        _audio.Play(SoundEffectTypes.PlayerHurt);
        _runStats.TrackDamageTaken(damage);

        if (Health > 0) return;

        Health = 0;
        MarkedForDeletion = true;
        _onDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        Health += amount;
        _runStats.TrackHealing(amount);
    }

    public event EventHandler<PlayerCharacter> OnExperienceGain = (_, _) => { };

    public void GainExperience(float amount)
    {
        Experience += amount * Stats.ExperienceMultiplier;
        OnExperienceGain(this, this);
        _runStats.TrackExperienceGained(amount);
    }

    public void DirectionInput(UnitVector2 input) => IntentVelocity = (Vector2)input * Stats.Speed;

    public override void Update(GameTime gameTime)
    {
        _invincibilityDuration -= gameTime.ElapsedGameTime;
        WeaponBelt.Update(gameTime);
        ApplyLifeSteal();
        _healthRegen.Update(gameTime, this);
        base.Update(gameTime);
    }

    private void ApplyLifeSteal()
    {
        if (_killsSinceLastLifeSteal < Stats.KillsPerHeal) return;

        Heal(1);
        _killsSinceLastLifeSteal -=
            Stats.KillsPerHeal; // Only consume the required number of kills rather than reset to 0
    }

    public void AddPowerUp(IPowerUp powerUp)
    {
        switch (powerUp)
        {
            case WeaponUnlock unlock:
                unlock.Apply(this);
                break;
            case MaxHealthUp maxHealthUp:
                Stats.AddPowerUp(maxHealthUp);
                Health += maxHealthUp.Value;
                break;
            case IPlayerPowerUp playerPowerUp:
                Stats.AddPowerUp(playerPowerUp);
                break;
            case IWeaponPowerUp weaponPowerUp:
                WeaponBelt.AddPowerUp(weaponPowerUp);
                break;
        }
    }

    public void OnKill(EnemyBase enemy)
    {
        _killsSinceLastLifeSteal += 1;
        MaybeExplodeOnKill(enemy);
        _runStats.TrackKill(enemy);
    }

    private void MaybeExplodeOnKill(EnemyBase enemy)
    {
        const int baseBullets = 4;
        var chance = Stats.EnemyDeathExplosionChance;

        if (chance <= 0f)
            return;

        var guaranteedExplosions = (int)MathF.Floor(chance);
        var remainderChance = chance - guaranteedExplosions;

        var explosionCount = guaranteedExplosions;

        if (Random.Shared.NextSingle() < remainderChance)
            explosionCount++;

        if (explosionCount <= 0)
            return;

        var bullets = baseBullets * explosionCount;
        _deathBlast.Explode(this, enemy.Position, bullets, WeaponBelt.Stats.DamageMultiplier);
    }
}