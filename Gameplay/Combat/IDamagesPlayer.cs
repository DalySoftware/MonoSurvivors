using Gameplay.CollisionDetection;

namespace Gameplay.Combat;

public interface IDamagesPlayer : IHasColliders
{
    public int Damage { get; }
}