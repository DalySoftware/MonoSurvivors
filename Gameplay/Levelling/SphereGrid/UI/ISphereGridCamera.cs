using Gameplay.Behaviour;

namespace Gameplay.Levelling.SphereGrid.UI;

public interface ISphereGridCamera
{
    IHasPosition? Target { get; set; }
    Matrix Transform { get; }
    Vector2 Position { get; set; }
    void Update(GameTime gameTime);
    Vector2 ScreenToWorld(Vector2 mouseScreenPosition);
    Vector2 WorldToScreen(Vector2 worldPosition);
}