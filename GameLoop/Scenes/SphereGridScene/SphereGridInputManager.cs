using GameLoop.Input;
using Gameplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.SphereGridScene;

internal class SphereGridInputManager(IGlobalCommands globalCommands) : BaseInputManager
{
    internal Vector2 CameraOffset { get; private set; } = Vector2.Zero;
    internal bool IsPanning { get; private set; }

    internal override void Update()
    {
        base.Update();

        if (WasPressedThisFrame(Keys.Escape) ||
            GamePadState.Buttons.Back == ButtonState.Pressed ||
            WasPressedThisFrame(Keys.Tab)) globalCommands.CloseSphereGrid();

        IsPanning = MouseState.MiddleButton == ButtonState.Pressed;
        if (IsPanning)
        {
            var mouseDelta = new Vector2(
                MouseState.X - PreviousMouseState.X,
                MouseState.Y - PreviousMouseState.Y
            );
            CameraOffset += mouseDelta;
        }

        // Handle panning with gamepad right thumbstick
        var thumbstickInput = GamePadState.ThumbSticks.Right;
        if (thumbstickInput.LengthSquared() > 0.02f)
            CameraOffset += new Vector2(-thumbstickInput.X, thumbstickInput.Y) * 8f;
    }
}