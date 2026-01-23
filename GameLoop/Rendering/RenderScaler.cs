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

    private readonly Effect? _postEffect;
    private readonly Vector2 _sourceTexel;

    public RenderScaler(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ContentManager content)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;

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

        _postEffect = content.Load<Effect>(Paths.ShaderEffects.Crt);
        _sourceTexel = new Vector2(1f / _renderTarget.Width, 1f / _renderTarget.Height);

        UpdateOutputRectangle();
    }


    public void Dispose() => _renderTarget.Dispose();

    public int Width => InternalWidth;
    public int Height => InternalHeight;

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

    public void BeginRenderTarget()
    {
        _graphicsDevice.SetRenderTarget(_renderTarget);
        _graphicsDevice.Clear(Color.Black);
    }

    public void EndRenderTarget()
    {
        _graphicsDevice.SetRenderTarget(null);
        _graphicsDevice.Clear(Color.Black);

        var effect = _postEffect;
        if (effect != null)
        {
            var vp = _graphicsDevice.Viewport;

            // Start with a basic ortho that maps pixel coords to clip space.
            var projection = Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, 1);
            effect.Parameters["MatrixTransform"]?.SetValue(projection);
            effect.Parameters["SourceTexel"]?.SetValue(_sourceTexel);

            effect.Parameters["ScanlineStrength"]?.SetValue(0.2f);
            effect.Parameters["ScanlineStepPx"]?.SetValue(4f);

            effect.Parameters["GrilleStrength"]?.SetValue(0.03f);
            effect.Parameters["GrilleStepPx"]?.SetValue(8f);

            effect.Parameters["VignetteStrength"]?.SetValue(0.9f);
            effect.Parameters["VignetteWidthX"]?.SetValue(0.06f);
            effect.Parameters["VignetteWidthY"]?.SetValue(0.08f);
            effect.Parameters["VignetteCurve"]?.SetValue(8f);
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