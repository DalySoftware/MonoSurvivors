using System;
using ContentLibrary;
using GameLoop.UI;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Rendering;

public sealed class RenderScaler : IRenderViewport, IDisposable
{
    private const int InternalWidth = 1920;
    private const int InternalHeight = 1080;

    private readonly RenderTarget2D _renderTarget;
    private Rectangle _outputRect;

    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;
    private readonly IBackground _background;

    private bool _enableCrt;
    private readonly Effect? _crtEffect;

    public RenderScaler(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager content,
        IBackground background)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
        _background = background;

        _renderTarget = new RenderTarget2D(
            _graphicsDevice,
            InternalWidth,
            InternalHeight,
            false,
            SurfaceFormat.Color,
            DepthFormat.None,
            0,
            RenderTargetUsage.DiscardContents
        );

        _crtEffect = content.Load<Effect>(Paths.ShaderEffects.Crt);
        var sourceTexel = new Vector2(1f / _renderTarget.Width, 1f / _renderTarget.Height);
        SetCrtConstantParameters(_crtEffect, sourceTexel, background.Color);
        _enableCrt = true;

        UpdateOutputRectangle();
    }

    public void Dispose() => _renderTarget.Dispose();

    public int Width => InternalWidth;
    public int Height => InternalHeight;

    private static void SetCrtConstantParameters(Effect effect, Vector2 sourceTexel, Color backgroundColor)
    {
        effect.Parameters["SourceTexel"]?.SetValue(sourceTexel);

        effect.Parameters["ScanlineStrength"]?.SetValue(0.25f);
        effect.Parameters["ScanlineStepPx"]?.SetValue(4f);

        effect.Parameters["GrilleStrength"]?.SetValue(0.01f);
        effect.Parameters["GrilleStepPx"]?.SetValue(8f);

        effect.Parameters["VignetteStrength"]?.SetValue(1f);
        effect.Parameters["VignetteWidthX"]?.SetValue(0.04f);
        effect.Parameters["VignetteWidthY"]?.SetValue(0.04f);
        effect.Parameters["VignetteCurve"]?.SetValue(3f);
        effect.Parameters["CornerBoost"]?.SetValue(1f);

        effect.Parameters["Gain"]?.SetValue(1f);

        effect.Parameters["BlurRadiusPx"]?.SetValue(2f);
        effect.Parameters["BlurStrength"]?.SetValue(0f);
        effect.Parameters["BloomStrength"]?.SetValue(0.2f);
        effect.Parameters["BloomThreshold"]?.SetValue(0.5f);
        effect.Parameters["BloomRadiusPx"]?.SetValue(2f);

        effect.Parameters["ChromaticBleedPx"]?.SetValue(2f);
        effect.Parameters["ChromaticBleedX"]?.SetValue(0.9f);
        effect.Parameters["ChromaStrength"]?.SetValue(1f);

        // Must match the background
        effect.Parameters["MatteColor"]?.SetValue(backgroundColor.ToVector4());
        effect.Parameters["MatteEdgeCurve"]?.SetValue(10f);
    }

    public void UpdateOutputRectangle()
    {
        var windowWidth = _graphicsDevice.PresentationParameters.BackBufferWidth;
        var windowHeight = _graphicsDevice.PresentationParameters.BackBufferHeight;

        const float targetAspect = (float)InternalWidth / InternalHeight;
        var windowAspect = (float)windowWidth / windowHeight;

        int outputWidth, outputHeight;
        if (windowAspect > targetAspect)
        {
            outputHeight = windowHeight;
            outputWidth = (int)(outputHeight * targetAspect);
        }
        else
        {
            outputWidth = windowWidth;
            outputHeight = (int)(outputWidth / targetAspect);
        }

        var offsetX = (windowWidth - outputWidth) / 2;
        var offsetY = (windowHeight - outputHeight) / 2;

        _outputRect = new Rectangle(offsetX, offsetY, outputWidth, outputHeight);
    }

    public void ToggleCrt() => _enableCrt = !_enableCrt;

    public void BeginRenderTarget()
    {
        _graphicsDevice.SetRenderTarget(_renderTarget);
        _graphicsDevice.Clear(_background.Color);
    }

    public void EndRenderTarget()
    {
        _graphicsDevice.SetRenderTarget(null);
        _graphicsDevice.Clear(_background.Color);

        var effect = _enableCrt ? _crtEffect : null;
        if (effect is not null)
        {
            var vp = _graphicsDevice.Viewport;

            // Basic ortho that maps pixel coords to clip space.
            var projection = Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, 1);
            effect.Parameters["MatrixTransform"]?.SetValue(projection);

            var r = _outputRect;
            effect.Parameters["OutputRectPx"]?.SetValue(new Vector4(r.X, r.Y, r.Width, r.Height));
        }

        _spriteBatch.Begin(
            SpriteSortMode.Immediate,
            BlendState.Opaque,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            effect
        );

        _spriteBatch.Draw(_renderTarget, _outputRect, Color.White);
        _spriteBatch.End();
    }

    public Vector2 ScreenToInternal(Vector2 screenPos) => new(
        (screenPos.X - _outputRect.X) * InternalWidth / _outputRect.Width,
        (screenPos.Y - _outputRect.Y) * InternalHeight / _outputRect.Height
    );

    public UiRectangle UiRectangle() => new(Vector2.Zero, new Vector2(InternalWidth, InternalHeight));
}