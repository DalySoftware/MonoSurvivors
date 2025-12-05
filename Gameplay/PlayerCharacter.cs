using ContentLibrary;
using Gameplay.Combat;
using Gameplay.Rendering;
using Gameplay.Utilities;

namespace Gameplay;

public class PlayerCharacter(Vector2 position) : MovableEntity(position), IDamageablePlayer, IVisual
{
    private const float Speed = 0.5f;

    public float Experience { get; set; } = 0f;

    public float Health
    {
        get;
        set
        {
            if (value <= 0)
                MarkedForDeletion = true;
            field = value;
        }
    } = 100f;

    public float CollisionRadius => 16f;
    public string TexturePath => Paths.Images.Player;

    public void DirectionInput(UnitVector2 input) => Velocity = (Vector2)input * Speed;
}