using GameLoop.Rendering;
using Gameplay;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Input;

internal sealed class GameInputState(RenderScaler renderScaler) : IMouseInputState
{
    internal KeyboardState KeyboardState { get; set; } = Keyboard.GetState();
    internal KeyboardState PreviousKeyboardState { get; set; } = Keyboard.GetState();
    internal GamePadState GamePadState { get; set; } = GamePad.GetState(0);
    internal GamePadState PreviousGamePadState { get; set; }

    public InputMethod CurrentInputMethod { get; set; }

    public MouseState MouseState
    {
        get;
        set => field = ConvertToInternal(value);
    } = Mouse.GetState();

    public MouseState PreviousMouseState { get; set; } = ConvertToInternal(renderScaler, Mouse.GetState());

    private MouseState ConvertToInternal(MouseState raw) => ConvertToInternal(renderScaler, raw);

    private static MouseState ConvertToInternal(RenderScaler renderScaler, MouseState raw)
    {
        var internalPos = renderScaler.ScreenToInternal(raw.Position.ToVector2()).ToPoint();
        return new MouseState(
            internalPos.X,
            internalPos.Y,
            raw.ScrollWheelValue,
            raw.LeftButton,
            raw.MiddleButton,
            raw.RightButton,
            raw.XButton1,
            raw.XButton2
        );
    }
}

internal enum InputMethod
{
    KeyboardMouse,
    Gamepad,
}