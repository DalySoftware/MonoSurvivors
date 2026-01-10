using Microsoft.Xna.Framework.Input;

namespace Gameplay;

public interface IMouseInputState
{
    MouseState MouseState { get; }
    MouseState PreviousMouseState { get; }
}