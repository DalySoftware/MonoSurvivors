using System;
using System.Collections.Generic;
using ContentLibrary;
using GameLoop.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Pause.UI;

public class VolumeControl : IUiElement
{
    private readonly Func<float> _getValue;
    private readonly Action<float> _setValue;

    private readonly HorizontalStack _stack;
    private readonly Label _valueLabel;

    private VolumeControl(
        ContentManager content,
        Button.Factory buttonFactory,
        Vector2 center,
        string label,
        Func<float> getValue,
        Action<float> setValue)
    {
        _getValue = getValue;
        _setValue = setValue;

        _stack = new HorizontalStack(center, 50);

        // Fixed-width label column
        _stack.AddChild(pos =>
            new FixedSizeContainer(
                pos,
                new Vector2(300, 0), // width fixed, height irrelevant for horizontal stack
                UiAnchor.CenterLeft,
                labelPos =>
                    new Label.Factory(content, Paths.Fonts.BoldPixels.Medium, label, layerDepth: 0.5f)
                        .Create(labelPos, UiAnchor.CenterLeft)
            ));

        // Have to tell the compiler we're assigning these 
        Label valueLabel = null!;
        Button decreaseButton = null!;
        Button increaseButton = null!;

        // Controls stack on the right
        _stack.AddChild(pos =>
        {
            var controlsStack = new HorizontalStack(pos, 10);

            decreaseButton = controlsStack.AddChild(p =>
                buttonFactory.Create("-", DecreaseVolume, p, UiAnchor.CenterLeft, true));

            valueLabel = controlsStack.AddChild(p =>
                new Label.Factory(content, Paths.Fonts.BoldPixels.Medium, GetValueText(),
                        alignment: TextAlignment.Right,
                        templateString: "100%",
                        layerDepth: 0.5f)
                    .Create(p, UiAnchor.CenterLeft));

            increaseButton = controlsStack.AddChild(p =>
                buttonFactory.Create("+", IncreaseVolume, p, UiAnchor.CenterLeft, true));

            return controlsStack;
        });

        // Single, obvious assignment point
        DecreaseButton = decreaseButton;
        IncreaseButton = increaseButton;
        _valueLabel = valueLabel;
    }


    internal Button DecreaseButton { get; }
    internal Button IncreaseButton { get; }

    /// <summary>Exposes the interactive buttons in order.</summary>
    internal IEnumerable<Button> Buttons
    {
        get
        {
            yield return DecreaseButton;
            yield return IncreaseButton;
        }
    }

    public UiRectangle Rectangle => _stack.Rectangle;

    public void Draw(SpriteBatch spriteBatch) => _stack.Draw(spriteBatch);

    internal void RefreshLabel() => _valueLabel.Text = GetValueText();

    private void DecreaseVolume()
    {
        _setValue(Math.Max(0f, _getValue() - 0.05f));
        _valueLabel.Text = GetValueText();
    }

    private void IncreaseVolume()
    {
        _setValue(Math.Min(1f, _getValue() + 0.05f));
        _valueLabel.Text = GetValueText();
    }

    private string GetValueText() => $"{(int)(_getValue() * 100)}%";

    internal sealed class Factory(
        ContentManager content,
        Button.Factory buttonFactory,
        string label,
        Func<float> getValue,
        Action<float> setValue)
    {
        public VolumeControl Create(Vector2 center) => new(
            content,
            buttonFactory,
            center,
            label,
            getValue,
            setValue
        );
    }
}