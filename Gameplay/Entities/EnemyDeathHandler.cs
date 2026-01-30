using Gameplay.Audio;
using Gameplay.Entities.Enemies;
using Gameplay.Rendering;
using Gameplay.Rendering.Effects;

namespace Gameplay.Entities;

/// <summary>
///     Handles non-player-related effects/events when enemy dies
/// </summary>
public class EnemyDeathHandler(
    IRequestDeathGlitch deathGlitch,
    CrtGlitchPulse crtGlitch,
    ExperienceSpawner experienceSpawner,
    IAudioPlayer audio)
{
    internal void ProcessDeath(EnemyBase enemy)
    {
        experienceSpawner.SpawnExperienceFor(enemy);
        audio.Play(SoundEffectTypes.EnemyDeath);

        var glitchAmount = MathHelper.Clamp(enemy.Stats.MaxHealth * 0.015f, 0.3f, 1f);
        crtGlitch.Trigger(glitchAmount);

        if (enemy is IVisual visual)
            deathGlitch.EnqueueDeathGlitch(visual);
    }
}