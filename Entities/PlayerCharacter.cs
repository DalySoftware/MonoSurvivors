using Entities.Combat;
using Entities.Utilities;

namespace Entities;

public class PlayerCharacter(Vector2 position) : MovableEntity(position), IDamageablePlayer
{
    private const float Speed = 0.5f;

    public float Health { get; set; } = 100f;
    public float CollisionRadius => 16f;

    public void DirectionInput(UnitVector2 input) => Velocity = (Vector2)input * Speed;
}