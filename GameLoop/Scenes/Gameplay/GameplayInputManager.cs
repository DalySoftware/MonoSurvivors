using GameLoop.Input;
using Gameplay;
using Gameplay.Entities;
using Gameplay.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.Gameplay;

internal class GameplayInputManager(
    PlayerCharacter player,
    IGlobalCommands globalCommands,
    GameInputState inputState,
    SceneManager sceneManager)
    : BaseInputManager(globalCommands, inputState, sceneManager)
{
    private readonly GameplayActionInput _actions = new(inputState);

    internal void Update(GameTime gameTime)
    {
        if (ShouldSkipInput()) return;

        if (_actions.WasPressed(GameplayAction.Pause))
        {
            GlobalCommands.ShowPauseMenu();
            return;
        }

        if (_actions.WasPressed(GameplayAction.OpenSphereGrid))
        {
            GlobalCommands.ShowSphereGrid();
            return;
        }

        var movement = _actions.GetMovement();
        player.DirectionInput(new UnitVector2(movement.X, movement.Y));

#if DEBUG
        if (InputState.KeyboardState.IsKeyDown(Keys.LeftControl) &&
            InputState.KeyboardState.IsKeyDown(Keys.OemPlus) &&
            InputState.PreviousKeyboardState.IsKeyDown(Keys.OemPlus))
            player.GainExperience(100f);
#endif
    }
}