namespace Entities.Levelling;

public class Experience(Vector2 position, float value) : MovableEntity(position)
{
    public float Value { get; set; } = value;
}