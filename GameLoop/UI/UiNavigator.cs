using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLoop.UI;

internal static class UiNavigator
{
    /// <summary>
    ///     Finds the next button to focus based on a direction and current focus.
    /// </summary>
    public static Button? FindNext(
        Button current,
        IReadOnlyList<Button> candidates,
        Direction direction)
    {
        if (candidates.Count == 0) return null;

        var validCandidates = (direction switch
        {
            Direction.Up => candidates.Where(b => b.Centre.Y < current.Centre.Y),
            Direction.Down => candidates.Where(b => b.Centre.Y > current.Centre.Y),
            Direction.Left => candidates.Where(b => b.Centre.X < current.Centre.X),
            Direction.Right => candidates.Where(b => b.Centre.X > current.Centre.X),
            _ => candidates,
        }).ToList();

        if (validCandidates.Count == 0) return current; // no change if nothing in that direction

        // Order by primary axis distance, then secondary axis distance
        IEnumerable<Button> orderedCandidates = direction switch
        {
            Direction.Up or Direction.Down =>
                validCandidates
                    .OrderBy(b => Math.Abs(b.Centre.Y - current.Centre.Y))
                    .ThenBy(b => Math.Abs(b.Centre.X - current.Centre.X)),
            Direction.Left or Direction.Right =>
                validCandidates
                    .OrderBy(b => Math.Abs(b.Centre.X - current.Centre.X))
                    .ThenBy(b => Math.Abs(b.Centre.Y - current.Centre.Y)),
            _ => validCandidates,
        };

        return orderedCandidates.First();
    }
}

internal enum Direction
{
    Up,
    Down,
    Left,
    Right,
}