using Autofac;
using GameLoop.Input;
using GameLoop.Rendering;
using GameLoop.UI;
using Microsoft.Xna.Framework;
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
        RenderScaler renderScaler,
        InputGate inputGate,
        Title.Factory titleFactory,
        HelpText.Factory helpTextFactory,
        WeaponSelect.Factory weaponSelectFactory,
        TitleInputManager.Factory inputFactory)
    {
        _spriteBatch = spriteBatch;
        _inputGate = inputGate;

        var topCentre = renderScaler.UiRectangle().AnchorForPoint(UiAnchor.TopCenter);

        _stack = new VerticalStack(topCentre + new Vector2(0f, 100f), 100f);

        _stack.AddChild(titleFactory.Create);
        var weaponSelect = _stack.AddChild(weaponSelectFactory.Create);
        _stack.AddChild(helpTextFactory.Create);

        _input = inputFactory.Create(weaponSelect);
    }

    public void Update(GameTime gameTime)
    {
        if (_inputGate.ShouldProcessInput())
            _input.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin(SpriteSortMode.FrontToBack, samplerState: SamplerState.PointClamp);

        _stack.Draw(_spriteBatch);

        _spriteBatch.End();
    }

    public void Dispose() { }

    internal static void ConfigureServices(ContainerBuilder builder)
    {
        builder.RegisterType<TitleScene>().InstancePerDependency();

        builder.RegisterType<Title.Factory>();
        builder.RegisterType<HelpText.Factory>();
        builder.RegisterType<WeaponSelect.Factory>();
        builder.RegisterType<WeaponPanelFactory>();
        builder.RegisterType<TitleInputManager.Factory>();
    }
}