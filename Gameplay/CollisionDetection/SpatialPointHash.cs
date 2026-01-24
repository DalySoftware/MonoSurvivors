using System;
using System.Collections.Generic;

namespace Gameplay.CollisionDetection;

// Point-insert spatial grid: each item goes in exactly one cell (by position).
public sealed class SpatialPointHash<T>(float cellSize)
{
    private readonly Dictionary<long, List<T>> _grid = [];
    private readonly Stack<List<T>> _listPool = new();
    private readonly List<long> _touchedKeys = new(512);

    public float CellSize => cellSize;

    public void Clear()
    {
        for (var i = 0; i < _touchedKeys.Count; i++)
        {
            var key = _touchedKeys[i];
            var list = _grid[key];
            list.Clear();
            _listPool.Push(list);
            _grid.Remove(key);
        }

        _touchedKeys.Clear();
    }

    private static long Key(int x, int y) => ((long)x << 32) ^ (uint)y;

    private (int x, int y) GetCell(Vector2 position) =>
        ((int)MathF.Floor(position.X / cellSize), (int)MathF.Floor(position.Y / cellSize));

    public void Insert(Vector2 position, T item)
    {
        var cell = GetCell(position);
        var key = Key(cell.x, cell.y);

        if (!_grid.TryGetValue(key, out var list))
        {
            if (!_listPool.TryPop(out list)) list = new List<T>(4);
            _grid[key] = list;
            _touchedKeys.Add(key);
        }

        list.Add(item);
    }

    public void QueryNearbyInto(Vector2 position, List<T> results, int cellRadius = 1)
    {
        results.Clear();

        var centerCell = GetCell(position);

        for (var x = -cellRadius; x <= cellRadius; x++)
        for (var y = -cellRadius; y <= cellRadius; y++)
        {
            var key = Key(centerCell.x + x, centerCell.y + y);
            if (_grid.TryGetValue(key, out var items))
                results.AddRange(items);
        }
    }
}