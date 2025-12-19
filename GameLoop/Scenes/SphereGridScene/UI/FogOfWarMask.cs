using System.Collections.Generic;
using System.Linq;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.SphereGridScene.UI;

internal sealed class FogOfWarMask
{
    private readonly static BlendState EraseBlend = new()
    {
        ColorSourceBlend = Blend.Zero,
        ColorDestinationBlend = Blend.InverseSourceAlpha,
        AlphaSourceBlend = Blend.Zero,
        AlphaDestinationBlend = Blend.InverseSourceAlpha,
    };

    private readonly GraphicsDevice _graphics;
    private readonly RenderTarget2D _fogTarget;
    private readonly Texture2D _circle;
    private readonly SpriteBatch _spriteBatch;
    private readonly int _baseVisionRadius;
    private readonly float _layerDepth;

    private List<Vector2> _visibleCentres = [];

    public FogOfWarMask(GraphicsDevice graphics, int baseVisionRadius, float layerDepth)
    {
        _graphics = graphics;
        _baseVisionRadius = baseVisionRadius;
        _layerDepth = layerDepth;
        _spriteBatch = new SpriteBatch(graphics);

        var vp = graphics.Viewport;
        _fogTarget = new RenderTarget2D(
            graphics, vp.Width, vp.Height,
            false, SurfaceFormat.Color, DepthFormat.None);

        _circle = PrimitiveRenderer.CreateSoftCircle(graphics, baseVisionRadius,
            PrimitiveRenderer.SoftCircleMaskType.InsideTransparent);
    }

    public void Rebuild(IEnumerable<Vector2> visibleCentres)
    {
        _visibleCentres = visibleCentres.ToList();

        _graphics.SetRenderTarget(_fogTarget);
        _graphics.Clear(Color.DarkSlateGray);

        // Punch holes
        _spriteBatch.Begin(blendState: EraseBlend);

        foreach (var pos in _visibleCentres)
            _spriteBatch.Draw(
                _circle,
                pos,
                origin: new Vector2(_baseVisionRadius),
                color: Color.White,
                layerDepth: _layerDepth + 0.01f);

        _spriteBatch.End();
        _graphics.SetRenderTarget(null);
    }

    public void Draw(SpriteBatch spriteBatch) =>
        spriteBatch.Draw(_fogTarget, Vector2.Zero, Color.DarkSlateGray, layerDepth: _layerDepth);

    public bool IsVisible(Vector2 screenPosition)
    {
        var radiusSquared = _baseVisionRadius * _baseVisionRadius;
        return _visibleCentres.Any(center => Vector2.DistanceSquared(center, screenPosition) <= radiusSquared);
    }
}