using System;
using System.Linq;
using GameLoop.Input;
using GameLoop.Scenes.SphereGridScene.UI;
using Gameplay;
using Gameplay.Levelling.SphereGrid;
using Microsoft.Xna.Framework;

namespace GameLoop.Scenes.SphereGridScene;

internal class SphereGridInputManager(
    IGlobalCommands globalCommands,
    GameInputState inputState,
    SphereGridUi ui)
{
    private readonly SphereGridActionInput _actions = new(inputState);

    internal void Update(GameTime gameTime)
    {
        if (_actions.WasPressed(SphereGridAction.Close))
            globalCommands.CloseSphereGrid();

        if (_actions.IsMousePanning()) ui.Camera.Position -= _actions.GetMousePanDelta();

        if (_actions.WasPressed(SphereGridAction.UnlockHovered))
            ui.UnlockHoveredNode();

        if (_actions.WasPressed(SphereGridAction.UnlockFocused))
            ui.UnlockFocussedNode();

        if (_actions.WasPressed(SphereGridAction.ResetCamera))
            ui.Camera.Position = Vector2.Zero;

        var pan = _actions.GetRightStickPan();
        if (pan != Vector2.Zero)
            ui.Camera.Position -= pan * 32f;

        ui.HideFocus = inputState.CurrentInputMethod is InputMethod.KeyboardMouse;

        UpdateHoveredNode();
        UpdateFocussedNode(gameTime);
    }

    private void UpdateHoveredNode() => ui.UpdateHoveredNode(_actions.GetMousePositionForHover());

    private void UpdateFocussedNode(GameTime gameTime)
    {
        var inputDirection = _actions.GetNavigationDirection(gameTime);
        if (inputDirection == Vector2.Zero)
            return;

        var currentPosition = ui.NodePositions[ui.FocusedNode];

        Node? bestCandidate = null;
        var bestScore = float.MinValue;

        var inputDirNormalized = Vector2.Normalize(inputDirection);

        const float minAngleCos = 0.5f; // ~60° cone (more forgiving)
        const float distanceBias = 1.5f; // strongly favour closer nodes

        var candidates = ui.NodePositions
            .Where(kvp => kvp.Key != ui.FocusedNode && ui.IsVisible(kvp.Value));

        foreach (var (candidate, position) in candidates)
        {
            var toNode = position - currentPosition;
            var distance = toNode.Length();

            if (distance <= 0.0001f)
                continue;

            var dir = toNode / distance;
            var alignment = Vector2.Dot(dir, inputDirNormalized);

            // Reject nodes too far off-axis
            if (alignment < minAngleCos)
                continue;

            // Alignment matters, but distance dominates
            var distanceScore = 1f / MathF.Pow(distance, distanceBias);
            var score = alignment * distanceScore;

            if (score > bestScore)
            {
                bestScore = score;
                bestCandidate = candidate;
            }
        }

        if (bestCandidate == null) return;

        ui.FocusedNode = bestCandidate;
        ui.HideFocus = false;
    }
}