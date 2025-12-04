namespace Entities.Combat;

internal interface IDamagesPlayer : IHasPosition
{
    public float Damage { get; }
    public float CollisionRadius { get; }
}