using System.Collections.Generic;
using ContentLibrary;
using Gameplay.Audio;
using Gameplay.CollisionDetection;
using Gameplay.Entities;
using Gameplay.Rendering;

namespace Gameplay.Levelling;

public class Experience : MovableEntity, IPickup, ISpriteVisual
{
    private readonly IAudioPlayer _audio;
    private readonly float _value;

    public Experience(Vector2 position, float value, IAudioPlayer audio) : base(position)
    {
        _audio = audio;
        _value = value;
        Colliders = [new CircleCollider(this, 16f)];
    }

    public IEnumerable<ICollider> Colliders { get; }

    public void OnPickupBy(PlayerCharacter player)
    {
        player.GainExperience(_value);
        _audio.Play(SoundEffectTypes.ExperiencePickup);
        MarkedForDeletion = true;
    }

    public float Layer => Layers.Pickups;

    public string TexturePath => _value switch
    {
        >= 5 => Paths.Images.Experience2,
        _ => Paths.Images.Experience,
    };

    public void AddVelocity(Vector2 velocity)
    {
        const float damping = 0.99f; // 0 = no momentum, 1 = full momentum
        IntentVelocity *= damping; // decay old velocity
        IntentVelocity += velocity; // add new pull
    }
}