using System;
using Gameplay.Behaviour;

namespace Gameplay.Rendering;

public class ChaseCamera(Vector2 viewportSize, IHasPosition target)
{
    public Vector2 Position { get; private set; } = target.Position;

    public Rectangle VisibleWorldBounds =>
        new((int)TopLeft.X, (int)TopLeft.Y, (int)viewportSize.X, (int)viewportSize.Y);

    private Vector2 TopLeft => Position - ViewportCentre;

    public Matrix Transform =>
        Matrix.CreateTranslation(-Position.X + ViewportCentre.X, -Position.Y + ViewportCentre.Y, 0);

    private Vector2 ViewportCentre => viewportSize / 2;
    public void Follow(GameTime gameTime) => Position = ExpDecay(gameTime);

    private Vector2 ExpDecay(GameTime gameTime)
    {
        const float decayRate = 0.0025f; // How aggressively the camera follows the target
        return target.Position +
               (Position - target.Position) * MathF.Exp(-decayRate * gameTime.ElapsedGameTime.Milliseconds);
    }
}