using GameLoop.UI;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Rendering;

/// <summary>
///     Handles rendering everything at a fixed internal resolution, scaling to the screen with letterboxing.
/// </summary>
public class RenderScaler : IRenderViewport
{
    private const int InternalWidth = 1920;
    private const int InternalHeight = 1080;

    private readonly RenderTarget2D _renderTarget;
    private Rectangle _outputRect;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;
    /// <summary>
    ///     Handles rendering everything at a fixed internal resolution, scaling to the screen with letterboxing.
    /// </summary>
    public RenderScaler(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
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

        UpdateOutputRectangle();
    }

    public int Width => InternalWidth;
    public int Height => InternalHeight;

    /// <summary>
    ///     Call when the window size changes, or after Initialize().
    /// </summary>
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

    /// <summary>
    ///     Begin rendering to the internal 1920x1080 render target.
    /// </summary>
    public void BeginRenderTarget()
    {
        _graphicsDevice.SetRenderTarget(_renderTarget);
        _graphicsDevice.Clear(Color.Black);
    }

    /// <summary>
    ///     End rendering to the internal render target and draw it scaled to the screen.
    /// </summary>
    public void EndRenderTarget()
    {
        _graphicsDevice.SetRenderTarget(null);
        _graphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.Opaque,
            SamplerState.PointClamp, // pixel-perfect; swap to LinearClamp if you want smooth scaling
            DepthStencilState.None,
            RasterizerState.CullNone
        );

        _spriteBatch.Draw(_renderTarget, _outputRect, Color.White);
        _spriteBatch.End();
    }

    /// <summary>
    ///     Converts screen coordinates to internal 1920x1080 coordinates.
    ///     Useful for UI hit testing.
    /// </summary>
    public Vector2 ScreenToInternal(Vector2 screenPos) => new(
        (screenPos.X - _outputRect.X) * InternalWidth / _outputRect.Width,
        (screenPos.Y - _outputRect.Y) * InternalHeight / _outputRect.Height
    );

    /// <summary>
    ///     Returns a UiRectangle representing the internal 1920x1080 space.
    ///     Use this for UI layout so it is independent of the actual screen size / scaling.
    /// </summary>
    public UiRectangle UiRectangle() => new(Vector2.Zero, new Vector2(InternalWidth, InternalHeight));
}