namespace Entities.Combat;

internal interface IDamageableEnemy : IHasPosition
{
    public float Health { get; set; }
    public float CollisionRadius { get; }
}