using Characters.Utilities;

namespace Characters.Behaviour.Movement;

public static class FollowCharacter
{
    extension(Character actor)
    {
        public Vector2 Follow(Character target, float speed = 1f) => 
            (Vector2)new UnitVector2(target.Position - actor.Position) * speed;
    }
}