using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Entities;
using Gameplay.Levelling.SphereGrid.UI;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
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
    private Texture2D _circle;
    private readonly SpriteBatch _spriteBatch;
    private readonly int _baseVisionRadius;
    private readonly ISphereGridCamera _camera;
    private readonly float _layerDepth;

    private List<Vector2> _visibleCentres = [];

    private readonly PlayerStats _playerStats;
    private readonly PrimitiveRenderer _primitiveRenderer;
    private float _visionRadiusMultiplier = 1f;

    public FogOfWarMask(GraphicsDevice graphics, IRenderViewport viewport, int baseVisionRadius,
        ISphereGridCamera camera, PlayerStats playerStats, PrimitiveRenderer primitiveRenderer)
    {
        _graphics = graphics;
        _camera = camera;
        _layerDepth = Layers.Fog;
        _spriteBatch = new SpriteBatch(graphics);
        _baseVisionRadius = baseVisionRadius;
        _playerStats = playerStats;
        _primitiveRenderer = primitiveRenderer;

        _fogTarget = new RenderTarget2D(
            graphics, viewport.Width, viewport.Height,
            false, SurfaceFormat.Color, DepthFormat.None);

        _circle = primitiveRenderer.CreateSoftCircle(VisionRadius,
            PrimitiveRenderer.SoftCircleMaskType.InsideTransparent);
    }

    private int VisionRadius => (int)(_baseVisionRadius * _visionRadiusMultiplier);

    public void Rebuild(IEnumerable<Vector2> visibleCentres)
    {
        _visibleCentres = visibleCentres.ToList();
        RebuildCircleIfNeeded();

        _graphics.SetRenderTarget(_fogTarget);
        _graphics.Clear(ColorPalette.DarkGray);

        // Punch holes
        _spriteBatch.Begin(blendState: EraseBlend, transformMatrix: _camera.Transform);

        foreach (var pos in _visibleCentres)
            _spriteBatch.Draw(
                _circle,
                pos,
                origin: new Vector2(_baseVisionRadius * _visionRadiusMultiplier),
                layerDepth: _layerDepth + 0.01f);

        _spriteBatch.End();
        _graphics.SetRenderTarget(null);
    }

    private void RebuildCircleIfNeeded()
    {
        if (Math.Abs(_playerStats.GridVisionMultiplier - _visionRadiusMultiplier) < 0.001f) return;

        _visionRadiusMultiplier = _playerStats.GridVisionMultiplier;
        _circle = _primitiveRenderer.CreateSoftCircle(VisionRadius,
            PrimitiveRenderer.SoftCircleMaskType.InsideTransparent);
    }

    public void Draw(SpriteBatch spriteBatch) =>
        spriteBatch.Draw(_fogTarget, Vector2.Zero, ColorPalette.Charcoal, layerDepth: _layerDepth);

    public bool IsVisible(Vector2 screenPosition)
    {
        var radiusSquared = _baseVisionRadius * _baseVisionRadius * _visionRadiusMultiplier * _visionRadiusMultiplier;
        return _visibleCentres.Any(center => Vector2.DistanceSquared(center, screenPosition) <= radiusSquared);
    }
}