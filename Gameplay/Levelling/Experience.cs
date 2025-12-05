using ContentLibrary;
using Gameplay.Behaviour;
using Gameplay.Rendering;

namespace Gameplay.Levelling;

public class Experience : MovableEntity, IPickup, IVisual
{
    private readonly GravitateToEntity _followEntity;

    public Experience(Vector2 position, float value, PlayerCharacter player) : base(position)
    {
        _followEntity = new GravitateToEntity(this, player);
        Value = value;
    }

    public float Value { get; init; }

    public float CollisionRadius => 8f;
    public string TexturePath => Paths.Images.Experience;

    public void OnPickupBy(PlayerCharacter player)
    {
        player.Experience += Value;
        MarkedForDeletion = true;
    }

    public override void Update(GameTime gameTime)
    {
        Velocity = _followEntity.CalculateVelocity();
        base.Update(gameTime);
    }
}