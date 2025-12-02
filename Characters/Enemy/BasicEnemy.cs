using Characters.Behaviour.Movement;

namespace Characters.Enemy;

public class BasicEnemy(Vector2 initialPosition, PlayerCharacter player) : Character(initialPosition)
{
    public override void UpdatePosition(GameTime gameTime)
    {
        Velocity = this.Follow(player, 0.1f);
        
        base.UpdatePosition(gameTime);
    }
}