using ContentLibrary;
using Gameplay.Audio;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Entities;
using Gameplay.Rendering;

namespace Gameplay.Levelling;

public class Experience : MovableEntity, IPickup, IVisual
{
    private readonly IAudioPlayer _audio;
    private readonly GravitateToEntity _followEntity;
    private readonly PlayerCharacter _player;
    private readonly float _value;

    public Experience(Vector2 position, float value, PlayerCharacter player, IAudioPlayer audio) : base(position)
    {
        _audio = audio;
        _followEntity = new GravitateToEntity(this, player);
        _value = value;
        _player = player;
        Collider = new CircleCollider(this, 16f);
    }

    public ICollider Collider { get; }

    public void OnPickupBy(PlayerCharacter player)
    {
        player.GainExperience(_value);
        _audio.Play(SoundEffectTypes.ExperiencePickup);
        MarkedForDeletion = true;
    }

    public string TexturePath => Paths.Images.Experience;

    public override void Update(GameTime gameTime)
    {
        Velocity = _followEntity.CalculateVelocity() * _player.PickupRadiusMultiplier;
        base.Update(gameTime);
    }
}