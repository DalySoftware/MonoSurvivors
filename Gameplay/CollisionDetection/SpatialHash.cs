using System.Collections.Generic;

namespace Gameplay.CollisionDetection;

internal class SpatialHash<T>(float cellSize)
    where T : IHasCollider
{
    private readonly Dictionary<(int, int), List<T>> _grid = [];

    public void Insert(T item)
    {
        var cells = GetCells(item);

        foreach (var cell in cells)
        {
            if (!_grid.TryGetValue(cell, out var list))
            {
                list = [];
                _grid[cell] = list;
            }

            list.Add(item);
        }
    }

    public IEnumerable<T> QueryNearby(Vector2 position, int cellRadius = 1)
    {
        var centerCell = GetCell(position);
        var seen = new HashSet<T>();

        for (var x = -cellRadius; x <= cellRadius; x++)
        for (var y = -cellRadius; y <= cellRadius; y++)
        {
            var cell = (centerCell.x + x, centerCell.y + y);
            if (!_grid.TryGetValue(cell, out var items))
                continue;

            foreach (var item in items)
                if (seen.Add(item))
                    yield return item;
        }
    }

    private (int x, int y) GetCell(Vector2 position) => ((int)(position.X / cellSize), (int)(position.Y / cellSize));

    private IEnumerable<(int x, int y)> GetCells(T item) => item.Collider switch
    {
        CircleCollider circle => GetCircleCells(circle),
        RectangleCollider rect => GetRectangleCells(rect),
        var unmatched => [GetCell(unmatched.Position)]
    };

    private IEnumerable<(int x, int y)> GetCircleCells(CircleCollider circle)
    {
        var radius = circle.CollisionRadius;
        var minCell = GetCell(new Vector2(circle.Position.X - radius, circle.Position.Y - radius));
        var maxCell = GetCell(new Vector2(circle.Position.X + radius, circle.Position.Y + radius));

        for (var x = minCell.x; x <= maxCell.x; x++)
        for (var y = minCell.y; y <= maxCell.y; y++)
            yield return (x, y);
    }

    private IEnumerable<(int x, int y)> GetRectangleCells(RectangleCollider rect)
    {
        var minCell = GetCell(new Vector2(rect.Left, rect.Top));
        var maxCell = GetCell(new Vector2(rect.Right, rect.Bottom));

        for (var x = minCell.x; x <= maxCell.x; x++)
        for (var y = minCell.y; y <= maxCell.y; y++)
            yield return (x, y);
    }
}