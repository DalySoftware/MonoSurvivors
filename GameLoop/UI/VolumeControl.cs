using System;
using ContentLibrary;
using Gameplay.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.UI;

public class VolumeControl
{
    private const float VolumeStep = 0.05f;

    private readonly Vector2 _centre;
    private readonly Button _decreaseButton;
    private readonly SpriteFont _font;
    private readonly Func<float> _getValue;
    private readonly Button _increaseButton;
    private readonly string _label;
    private readonly Action<float> _setValue;

    public VolumeControl(
        ContentManager content,
        PrimitiveRenderer primitiveRenderer,
        Vector2 centre,
        string label,
        Func<float> getValue,
        Action<float> setValue)
    {
        _centre = centre;
        _label = label;
        _getValue = getValue;
        _setValue = setValue;
        _font = content.Load<SpriteFont>(Paths.Fonts.BoldPixels.Medium);

        // Create buttons
        const int buttonRelativeX = 200;
        _decreaseButton = new Button(content, primitiveRenderer, new Vector2(centre.X + buttonRelativeX, centre.Y), "-",
            DecreaseVolume,
            true);
        const float valuePadding = 20f;
        var maxValueSize = _font.MeasureString("100%");
        _increaseButton = new Button(content, primitiveRenderer,
            new Vector2(_decreaseButton.Centre.X + _decreaseButton.Size.X + maxValueSize.X + valuePadding * 2,
                centre.Y),
            "+",
            IncreaseVolume, true);
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

    public void Update(MouseState mouseState)
    {
        _decreaseButton.Update(mouseState);
        _increaseButton.Update(mouseState);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Center of the block between buttons
        var blockCentre = (_decreaseButton.Centre + _increaseButton.Centre) * 0.5f;

        // Reserve max width for value
        var valueText = $"{(int)(_getValue() * 100)}%";

        // Compute positions
        var blockLeftX = blockCentre.X - 400f;

        // --- Label (left-aligned, vertically centered)
        var labelSize = _font.MeasureString(_label);
        var labelOrigin = new Vector2(0f, labelSize.Y * 0.5f); // left align
        var labelPosition = new Vector2(blockLeftX, _centre.Y);
        spriteBatch.DrawString(_font, _label, labelPosition, origin: labelOrigin, layerDepth: .4f);

        // --- Decrease button
        _decreaseButton.Draw(spriteBatch);

        // --- Volume (right-aligned, vertically centered)
        var volumeSize = _font.MeasureString(valueText);
        var volumeOrigin = new Vector2(volumeSize.X, volumeSize.Y * 0.5f); // right align
        var volumePosition = blockCentre + volumeSize * new Vector2(0.5f, 0);
        spriteBatch.DrawString(_font, valueText, volumePosition, origin: volumeOrigin, layerDepth: .5f);

        // --- Increase button
        _increaseButton.Draw(spriteBatch);
    }
}