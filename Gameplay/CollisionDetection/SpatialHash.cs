using System.Collections.Generic;

namespace Gameplay.CollisionDetection;

internal class SpatialHash<T>(float cellSize)
    where T : ICircleCollider
{
    private readonly Dictionary<(int, int), List<T>> _grid = [];

    public void Insert(T item)
    {
        var cell = GetCell(item.Position);
        if (!_grid.TryGetValue(cell, out var list))
        {
            list = [];
            _grid[cell] = list;
        }
        list.Add(item);
    }

    public IEnumerable<T> QueryNearby(Vector2 position, int cellRadius = 1)
    {
        var centerCell = GetCell(position);

        for (var x = -cellRadius; x <= cellRadius; x++)
        {
            for (var y = -cellRadius; y <= cellRadius; y++)
            {
                var cell = (centerCell.x + x, centerCell.y + y);
                if (!_grid.TryGetValue(cell, out var items))
                    continue;
                
                foreach (var item in items)
                    yield return item;
            }
        }
    }

    private (int x, int y) GetCell(Vector2 position)
    {
        return ((int)(position.X / cellSize), (int)(position.Y / cellSize));
    }
}
