namespace Entities.Combat;

internal interface IDamagesEnemies : IHasPosition
{
    public float Damage { get; }
    public float CollisionRadius { get; }
    public void OnHit() { }
}