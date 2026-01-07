using System.Collections.Generic;
using System.Linq;
using ContentLibrary;
using GameLoop.Input;
using GameLoop.UI;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Title;

internal class HelpText(Label.Factory labelFactory, GameInputState inputState, UiRectangle rectangle) : IUiElement
{
    private const string FontPath = Paths.Fonts.BoldPixels.Large;

    private readonly static Dictionary<InputMethod, string> Text = new()
    {
        [InputMethod.Gamepad] = "[A] to start | [B] to exit",
        [InputMethod.KeyboardMouse] = "[Space] to start | [Esc] to exit",
    };

    public UiRectangle Rectangle { get; } = rectangle;
    public void Draw(SpriteBatch spriteBatch)
    {
        var label = labelFactory.Create(FontPath, Text[inputState.CurrentInputMethod], Rectangle.Origin,
            Rectangle.OriginAnchor, ColorPalette.LightGray, layerDepth: 1f);
        label.Draw(spriteBatch);
    }

    internal class Factory(Label.Factory labelFactory, GameInputState inputState)
    {
        internal HelpText Create(Vector2 anchor)
        {
            var size = Measure();
            var rectangle = new UiRectangle(anchor, size, UiAnchor.TopCenter);
            return new HelpText(labelFactory, inputState, rectangle);
        }

        private Vector2 Measure()
        {
            var biggestText = Text.MaxBy(t => t.Value.Length).Value;
            return labelFactory.Measure(FontPath, biggestText);
        }
    }
}