using System;
using ContentLibrary;
using Gameplay.Rendering;
using Gameplay.Rendering.Colors;
using Gameplay.Rendering.Effects;
using Gameplay.Telemetry;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Background;

public class BackgroundRenderer
{
    private readonly static Color BackgroundColor = ColorPalette.White;

    private readonly ChaseCamera _camera;
    private readonly DecalLayer _decalLayer;
    private readonly PerformanceMetrics _perf;

    private readonly Texture2D _backgroundTile;
    private readonly Effect _backgroundEffect;
    private readonly Effect _greyscaleEffect;

    public BackgroundRenderer(ContentManager content, ChaseCamera camera, DecalLayer decalLayer,
        PerformanceMetrics perf)
    {
        _camera = camera;
        _decalLayer = decalLayer;
        _perf = perf;

        _backgroundTile = content.Load<Texture2D>(Paths.Images.BackgroundTile);

        _greyscaleEffect = content.Load<Effect>(Paths.ShaderEffects.Greyscale);
        _greyscaleEffect.Parameters["Saturation"].SetValue(0.4f);

        _backgroundEffect = content.Load<Effect>(Paths.ShaderEffects.BackgroundFloor);

        var hashTexture = CreateHashTexture(_backgroundTile.GraphicsDevice, 256, Random.Shared.Next());
        _backgroundEffect.Parameters["HashTexture"].SetValue(hashTexture);
        _backgroundEffect.Parameters["HashSize"].SetValue(256f);

        _backgroundEffect.Parameters["Saturation"].SetValue(0.7f);
        _backgroundEffect.Parameters["Gain"].SetValue(0.7f);

        _backgroundEffect.Parameters["TileSizePx"].SetValue((float)_backgroundTile.Width);

        _backgroundEffect.Parameters["SpeckSizePx"].SetValue(4f);
        _backgroundEffect.Parameters["SpeckDensity"].SetValue(0.045f);
        _backgroundEffect.Parameters["SpeckStrength"].SetValue(0.35f);
        _backgroundEffect.Parameters["SpeckColor"].SetValue(ColorPalette.Brown.ToVector3());

        _backgroundEffect.Parameters["DashChance"].SetValue(0.95f);
        _backgroundEffect.Parameters["DashMinCells"].SetValue(2f);
        _backgroundEffect.Parameters["DashMaxCells"].SetValue(4f);

        _backgroundEffect.Parameters["Seed"].SetValue(new Vector2(
            Random.Shared.Next(0, 256),
            Random.Shared.Next(0, 256)));
    }

    private static Texture2D CreateHashTexture(GraphicsDevice device, int size, int seed)
    {
        var rng = new Random(seed);
        var data = new Color[size * size];

        for (var i = 0; i < data.Length; i++)
            data[i] = new Color(
                (byte)rng.Next(256),
                (byte)rng.Next(256),
                (byte)rng.Next(256),
                (byte)rng.Next(256));

        var tex = new Texture2D(device, size, size, false, SurfaceFormat.Color);
        tex.SetData(data);
        return tex;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        using var _ = _perf.MeasureProbe("BG");

        // Update per-draw world mapping
        // Keep in sync with shader constants:
        // WrapCell span 16384 and SpeckSizePx 4 => 65536 world px repeat.
        const int wrapPx = 16384 * 4;

        var bounds = _camera.VisibleWorldBounds;
        const float margin = CameraShake.MaxShakePx + 10;
        bounds.Inflate(margin, margin);

        var wrappedOrigin = new Vector2(Mod(bounds.Left, wrapPx), Mod(bounds.Top, wrapPx));

        _backgroundEffect.Parameters["WorldOrigin"].SetValue(wrappedOrigin);
        _backgroundEffect.Parameters["WorldSize"].SetValue(new Vector2(bounds.Width, bounds.Height));

        spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: _camera.Transform,
            sortMode: SpriteSortMode.Deferred,
            effect: _backgroundEffect);

        spriteBatch.Draw(_backgroundTile, bounds, BackgroundColor);
        spriteBatch.End();

        spriteBatch.Begin(
            samplerState: SamplerState.PointWrap,
            transformMatrix: _camera.Transform,
            sortMode: SpriteSortMode.FrontToBack,
            effect: _greyscaleEffect);

        _decalLayer.Draw(spriteBatch);
        spriteBatch.End();
    }

    // Wraps for negatives, unlike %
    private static int Mod(int x, int m) => (x % m + m) % m;
}