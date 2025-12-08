using System;
using GameLoop.Input;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.SphereGridScene;

internal class SphereGridInputManager : BaseInputManager
{
    internal Action OnClose { get; init; } = () => { };
    
    // Initialise with current state to avoid immediate close on first frame
    private KeyboardState _previousKeyboardState = Keyboard.GetState();

    internal override void Update()
    {
        var keyboardState = Keyboard.GetState();

        // Close grid with Tab or Escape
        if (keyboardState.IsKeyDown(Keys.Tab) && !_previousKeyboardState.IsKeyDown(Keys.Tab) ||
            keyboardState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
        {
            OnClose();
        }

        _previousKeyboardState = keyboardState;
    }
}
