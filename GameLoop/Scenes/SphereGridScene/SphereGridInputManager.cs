using System;
using GameLoop.Input;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Scenes.SphereGridScene;

internal class SphereGridInputManager : BaseInputManager
{
    internal Action OnClose { get; init; } = () => { };

    internal override void Update()
    {
        base.Update();

        if (WasPressedThisFrame(Keys.Tab)) OnClose();
    }
}