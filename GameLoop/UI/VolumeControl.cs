using System;
using ContentLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.UI;

public class VolumeControl : UiElement
{
    private readonly string _label;
    private readonly Func<float> _getValue;
    private readonly Action<float> _setValue;
    private readonly SpriteFont _font;
    private readonly Button _decreaseButton;
    private readonly Button _increaseButton;
    private readonly PanelRenderer _panelRenderer;

    private const float VolumeStep = 0.1f;

    public VolumeControl(
        ContentManager content,
        Vector2 position,
        string label,
        Func<float> getValue,
        Action<float> setValue)
    {
        Position = position;
        _label = label;
        _getValue = getValue;
        _setValue = setValue;
        _font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Medium);
        _panelRenderer = new PanelRenderer(content);

        // Create buttons
        _decreaseButton = new Button(content, new Vector2(position.X + 400, position.Y), new Vector2(60, 60), "-",
            DecreaseVolume);
        _increaseButton = new Button(content, new Vector2(position.X + 540, position.Y), new Vector2(60, 60), "+",
            IncreaseVolume);
    }

    private void DecreaseVolume()
    {
        var newValue = Math.Max(0f, _getValue() - VolumeStep);
        _setValue(newValue);
    }

    private void IncreaseVolume()
    {
        var newValue = Math.Min(1f, _getValue() + VolumeStep);
        _setValue(newValue);
    }

    public void Update(MouseState mouseState, MouseState previousMouseState)
    {
        if (!IsVisible) return;

        _decreaseButton.Update(mouseState, previousMouseState);
        _increaseButton.Update(mouseState, previousMouseState);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        // Draw label
        spriteBatch.DrawString(_font, _label, Position, Color.White, 0f, Vector2.Zero, 1f,
            SpriteEffects.None, 0.05f);

        // Draw volume percentage
        var volumeText = $"{(int)(_getValue() * 100)}%";
        var volumeSize = _font.MeasureString(volumeText);
        var volumePosition = new Vector2(Position.X + 470, Position.Y + 20);
        spriteBatch.DrawString(_font, volumeText, volumePosition, Color.White, 0f, Vector2.Zero, 1f,
            SpriteEffects.None, 0.05f);

        // Draw buttons
        _decreaseButton.Draw(spriteBatch);
        _increaseButton.Draw(spriteBatch);
    }
}
