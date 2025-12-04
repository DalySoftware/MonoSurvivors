namespace Entities.Combat;

internal interface IDamageablePlayer : IHasPosition
{
    public float Health { get; set; }
    public float CollisionRadius { get; }
}