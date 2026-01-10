using System;
using Gameplay.Behaviour;
using Gameplay.Levelling.SphereGrid.UI;

namespace Gameplay.Rendering;

public class ChaseCamera(IRenderViewport viewport, IHasPosition? target, float decayRate = 0.0025f) : ISphereGridCamera
{
    private Vector2 ViewportSize => new(viewport.Width, viewport.Height);
    private Vector2 ViewportCentre => ViewportSize / 2;
    private Vector2 TopLeft => Position - ViewportCentre;

    public Rectangle VisibleWorldBounds =>
        new((int)TopLeft.X, (int)TopLeft.Y, (int)ViewportSize.X, (int)ViewportSize.Y);

    public IHasPosition? Target { get; set; } = target;
    public Vector2 Position { get; set; } = target?.Position ?? Vector2.Zero;

    public Matrix Transform =>
        Matrix.CreateTranslation(-Position.X + ViewportCentre.X, -Position.Y + ViewportCentre.Y, 0);

    public void Update(GameTime gameTime) => Position = ExpDecay(gameTime);

    public Vector2 ScreenToWorld(Vector2 screenPosition) => Vector2.Transform(screenPosition, Matrix.Invert(Transform));
    public Vector2 WorldToScreen(Vector2 worldPosition) => Vector2.Transform(worldPosition, Transform);

    private Vector2 ExpDecay(GameTime gameTime)
    {
        var anchor = Target?.Position ?? Vector2.Zero;
        return anchor + (Position - anchor) * MathF.Exp(-decayRate * gameTime.ElapsedGameTime.Milliseconds);
    }
}