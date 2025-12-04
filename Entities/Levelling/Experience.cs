using Entities.Behaviour;

namespace Entities.Levelling;

public class Experience : MovableEntity
{
    private readonly GravitateToEntity _followEntity;

    public Experience(Vector2 position, float value, PlayerCharacter player) : base(position)
    {
        _followEntity = new GravitateToEntity(this, player);
        Value = value;
    }

    public float Value { get; init; }

    public override void Update(GameTime gameTime)
    {
        Velocity = _followEntity.CalculateVelocity();
        base.Update(gameTime);
    }
}