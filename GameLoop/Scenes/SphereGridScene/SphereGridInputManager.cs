using GameLoop.Input;
using GameLoop.Scenes.SphereGridScene.UI;
using Gameplay;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.SphereGridScene;

internal class SphereGridInputManager(IGlobalCommands globalCommands, SphereGridUi ui)
    : BaseInputManager
{
    private bool _isPanning;

    internal override void Update()
    {
        base.Update();

        if (WasPressedThisFrame(Keys.Escape) ||
            GamePadState.Buttons.Back == ButtonState.Pressed ||
            WasPressedThisFrame(Keys.Tab)) globalCommands.CloseSphereGrid();

        ui.UpdateHoveredNode(MouseState.Position.ToVector2());

        _isPanning = MouseState.MiddleButton == ButtonState.Pressed;
        if (_isPanning)
        {
            var mouseDelta = new Vector2(
                MouseState.X - PreviousMouseState.X,
                MouseState.Y - PreviousMouseState.Y
            );
            ui.ScreenSpaceOrigin += mouseDelta;
        }

        if (MouseState.LeftButton == ButtonState.Pressed &&
            PreviousMouseState.LeftButton == ButtonState.Released)
            ui.UnlockHoveredNode();

        // Handle panning with gamepad right thumbstick
        var thumbstickInput = GamePadState.ThumbSticks.Right;
        if (thumbstickInput.LengthSquared() > 0.02f)
            ui.ScreenSpaceOrigin += new Vector2(-thumbstickInput.X, thumbstickInput.Y) * 8f;
    }
}