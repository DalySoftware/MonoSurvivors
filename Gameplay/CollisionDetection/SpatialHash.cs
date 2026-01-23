using System;
using System.Collections.Generic;

namespace Gameplay.CollisionDetection;

public sealed class SpatialHash<T>(float cellSize)
    where T : IHasColliders
{
    private readonly Dictionary<long, List<T>> _grid = [];
    private readonly Stack<List<T>> _listPool = new();
    private readonly List<long> _touchedKeys = new(512);

    private readonly HashSet<T> _seenItems = [];

    internal float CellSize => cellSize;

    public void Clear()
    {
        // Return used lists to pool
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

    private void AddToCell(int x, int y, T item)
    {
        var key = Key(x, y);
        if (!_grid.TryGetValue(key, out var list))
        {
            if (!_listPool.TryPop(out list)) list = new List<T>(4);

            _grid[key] = list;
            _touchedKeys.Add(key);
        }

        list.Add(item);
    }

    public void Insert(T item)
    {
        foreach (var collider in item.Colliders)
            switch (collider)
            {
                case CircleCollider circle:
                {
                    var r = circle.CollisionRadius;
                    var min = GetCell(new Vector2(circle.Position.X - r, circle.Position.Y - r));
                    var max = GetCell(new Vector2(circle.Position.X + r, circle.Position.Y + r));
                    for (var x = min.x; x <= max.x; x++)
                    for (var y = min.y; y <= max.y; y++)
                        AddToCell(x, y, item);
                    break;
                }
                case RectangleCollider rect:
                {
                    var min = GetCell(new Vector2(rect.Left, rect.Top));
                    var max = GetCell(new Vector2(rect.Right, rect.Bottom));
                    for (var x = min.x; x <= max.x; x++)
                    for (var y = min.y; y <= max.y; y++)
                        AddToCell(x, y, item);
                    break;
                }
                default:
                {
                    var cell = GetCell(collider.Position);
                    AddToCell(cell.x, cell.y, item);
                    break;
                }
            }
    }

    public void QueryNearbyInto(Vector2 position, List<T> results, int cellRadius = 1)
    {
        results.Clear();

        var centerCell = GetCell(position);
        _seenItems.Clear();

        for (var x = -cellRadius; x <= cellRadius; x++)
        for (var y = -cellRadius; y <= cellRadius; y++)
        {
            var key = Key(centerCell.x + x, centerCell.y + y);
            if (!_grid.TryGetValue(key, out var items))
                continue;

            foreach (var item in items)
                if (_seenItems.Add(item))
                    results.Add(item);
        }
    }


    private (int x, int y) GetCell(Vector2 position) =>
        ((int)MathF.Floor(position.X / cellSize), (int)MathF.Floor(position.Y / cellSize));
}