using GameLoop.Input;
using GameLoop.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace GameLoop.Scenes.Title;

internal class TitleScene : IScene
{
    private readonly SpriteBatch _spriteBatch;
    private readonly TitleInputManager _input;
    private readonly InputGate _inputGate;
    private readonly VerticalStack _stack;
    public TitleScene(SpriteBatch spriteBatch,
        ContentManager content,
        TitleInputManager input,
        InputGate inputGate)
    {
        _spriteBatch = spriteBatch;
        _input = input;
        _inputGate = inputGate;

        var topCentre = _spriteBatch
            .GraphicsDevice
            .Viewport
            .UiRectangle()
            .AnchorForPoint(UiAnchor.TopCenter);

        _stack = new VerticalStack(topCentre + new Vector2(0f, 100f), 100f);

        var titleFactory = new Title.Factory(content);
        _stack.AddChild(position => titleFactory.Create(position));
    }

    public void Update(GameTime gameTime)
    {
        if (_inputGate.ShouldProcessInput())
            _input.Update();
    }

    public void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin(SpriteSortMode.FrontToBack);

        _stack.Draw(_spriteBatch);

        _spriteBatch.End();
    }

    public void Dispose() { }
}