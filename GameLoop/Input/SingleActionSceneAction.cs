using GameLoop.Input.Mapping;

namespace GameLoop.Input;

public enum SingleActionSceneAction
{
    Exit,
    PrimaryAction,
}

internal sealed class SingleActionSceneActionInput(
    GameInputState state,
    ActionKeyMap<SingleActionSceneAction> map)
    : ActionInputBase<SingleActionSceneAction>(state, map);