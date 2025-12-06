using ContentLibrary;
using Gameplay.Audio;
using Gameplay.Behaviour;
using Gameplay.Entities;
using Gameplay.Rendering;

namespace Gameplay.Levelling;

public class Experience : MovableEntity, IPickup, IVisual
{
    private readonly IAudioPlayer _audio;
    private readonly GravitateToEntity _followEntity;

    public Experience(Vector2 position, float value, PlayerCharacter player, IAudioPlayer audio) : base(position)
    {
        _audio = audio;
        _followEntity = new GravitateToEntity(this, player);
        Value = value;
    }

    public float Value { get; init; }

    public float CollisionRadius => 8f;

    public void OnPickupBy(PlayerCharacter player)
    {
        player.Experience += Value;
        _audio.Play(SoundEffectTypes.ExperiencePickup);
        MarkedForDeletion = true;
    }

    public string TexturePath => Paths.Images.Experience;

    public override void Update(GameTime gameTime)
    {
        Velocity = _followEntity.CalculateVelocity();
        base.Update(gameTime);
    }
}