using Microsoft.Xna.Framework;

namespace GameLoop.UI;

public readonly record struct PointerInput(Vector2 Position, bool Pressed, bool Released);