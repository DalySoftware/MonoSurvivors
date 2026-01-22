using ContentLibrary;
using GameLoop.Input;
using GameLoop.UI;
using Gameplay;
using Gameplay.Rendering.Colors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLoop.Scenes.Title;

internal class HelpText(
    Label.Factory labelFactory,
    GameInputState inputState,
    UiRectangle rectangle,
    IAppLifeCycle appLifeCycle) : IUiElement
{
    private const string FontPath = Paths.Fonts.BoldPixels.Large;

    public UiRectangle Rectangle { get; } = rectangle;
    public void Draw(SpriteBatch spriteBatch)
    {
        var label = labelFactory.Create(FontPath, GetInstructionsText(), Rectangle.Origin,
            Rectangle.OriginAnchor, ColorPalette.LightGray, layerDepth: 1f);
        label.Draw(spriteBatch);
    }

    private string GetInstructionsText() => inputState.CurrentInputMethod switch
    {
        InputMethod.KeyboardMouse when appLifeCycle.CanExit => "[Space] to start | [Esc] to exit",
        InputMethod.KeyboardMouse => "[Space] to start",
        InputMethod.Gamepad when appLifeCycle.CanExit => "[A] to start | [B] to exit",
        InputMethod.Gamepad => "[A] to start",
        _ => string.Empty,
    };

    internal class Factory(Label.Factory labelFactory, GameInputState inputState, IAppLifeCycle appLifeCycle)
    {
        internal HelpText Create(Vector2 anchor)
        {
            var size = Measure();
            var rectangle = new UiRectangle(anchor, size, UiAnchor.TopCenter);
            return new HelpText(labelFactory, inputState, rectangle, appLifeCycle);
        }

        private Vector2 Measure()
        {
            const string biggestText = "[Space] to start | [Esc] to exit"; // hardcode this for now
            return labelFactory.Measure(FontPath, biggestText);
        }
    }
}