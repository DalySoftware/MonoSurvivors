using Gameplay.Levelling.PowerUps.Player;

namespace Gameplay.Entities;

public class PlayerStats
{
    public const int BaseHealth = 6;
    private const float BaseSpeed = 0.25f;
    private int _lifeSteal = 0;
    private float _speedMultiplier = 1f;

    public float DodgeChance { get; private set; } = 0f;
    public float HealthRegen { get; private set; } = 0f;
    public float ExperienceMultiplier { get; private set; } = 1f;
    public int EnemyDeathExplosionBullets { get; private set; } = 0;
    public int KillsPerHeal => _lifeSteal == 0 ? int.MaxValue : 100 / _lifeSteal;
    public int MaxHealth { get; private set; } = BaseHealth;
    public float PickupRadiusMultiplier { get; private set; } = 1f;
    public float Speed => BaseSpeed * _speedMultiplier;

    internal void AddPowerUp(IPlayerPowerUp powerUp)
    {
        switch (powerUp)
        {
            case DodgeChanceUp dodgeChanceUp:
                DodgeChance += dodgeChanceUp.Value;
                break;
            case ExperienceUp experienceUp:
                ExperienceMultiplier += experienceUp.Value;
                break;
            case ExplodeOnKillUp explodeOnKillUp:
                EnemyDeathExplosionBullets += explodeOnKillUp.Bullets;
                break;
            case HealthRegenUp healthRegenUp:
                HealthRegen += healthRegenUp.Value;
                break;
            case LifeStealUp lifeStealUp:
                _lifeSteal += lifeStealUp.Value;
                break;
            case MaxHealthUp maxHealthUp:
                MaxHealth += maxHealthUp.Value;
                break;
            case PickupRadiusUp radiusUp:
                PickupRadiusMultiplier += radiusUp.Value;
                break;
            case SpeedUp speedUp:
                _speedMultiplier += speedUp.Value;
                break;
        }
    }
}