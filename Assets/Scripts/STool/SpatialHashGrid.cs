using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace STool
{
    public class SpatialHashGrid<T> : IEnumerable<T>
    {
        private readonly float _cellSize;
        private readonly Dictionary<Vector2Int, List<T>> _grid = new();
        public SpatialHashGrid(float cellSize)
        {
            this._cellSize = cellSize;
        }
        public Vector2Int GetCell(Vector3 position)
        {
            return new Vector2Int(
                Mathf.RoundToInt(position.x / _cellSize),
                // Mathf.FloorToInt(position.y / _cellSize),
                Mathf.RoundToInt(position.z / _cellSize));
        }
        public void Insert(T item, Bounds bounds)
        {
            var min = GetCell(bounds.min);
            var max = GetCell(bounds.max);

            for (var x = min.x; x <= max.x; x++)
            // for (var y = min.y; y <= max.y; y++)
            for (var z = min.y; z <= max.y; z++)
            {
                var cell = new Vector2Int(x, z); //new Vector3Int(x, y, z);
                if (!_grid.TryGetValue(cell, out var list))
                    _grid[cell] = list = new List<T>();
                list.Add(item);
                // Debug.Log($"Grid Inserted: {cell}({bounds.center}-{bounds.size}) => {list.Count}");
            }
        }
        public void Remove(T item)
        {
            foreach (var pair in _grid)
            {
                pair.Value.Remove(item);
            }
        }
        public void Query(Vector3 position, float radius, List<T> results)
        {
            var min = GetCell(position - Vector3.one * radius);
            var max = GetCell(position + Vector3.one * radius);

            for (var x = min.x; x <= max.x; x++)
            // for (int y = min.y; y <= max.y; y++)
            for (var z = min.y; z <= max.y; z++)
            {
                var cell = new Vector2Int(x, z);//new Vector3Int(x, y, z);
                if (_grid.TryGetValue(cell, out var list))
                {
                    // Debug.Log($"Grid Found: {list.Count}");
                    results.AddRange(list);
                }
            }
        }
        public void Query(Vector3 position, float radius, HashSet<T>  results)
        {
            var min = GetCell(position - Vector3.one * radius);
            var max = GetCell(position + Vector3.one * radius);
            for (var x = min.x; x <= max.x; x++)
                // for (int y = min.y; y <= max.y; y++)
            for (var z = min.y; z <= max.y; z++)
            {
                var cell = new Vector2Int(x, z);//new Vector3Int(x, y, z);
                if (_grid.TryGetValue(cell, out var list))
                {
                    // Debug.Log($"Grid Found: {list.Count}");
                    results.UnionWith(list);
                }
            }
        }
        public void QueryAABB(Vector3 from, Vector3 to, HashSet<T> results)
        {
            var min = GetCell(from);
            var max = GetCell(to);
            for (var x = min.x; x <= max.x; x++)
                // for (int y = min.y; y <= max.y; y++)
            for (var z = min.y; z <= max.y; z++)
            {
                var cell = new Vector2Int(x, z);//new Vector3Int(x, y, z);
                if (_grid.TryGetValue(cell, out var list))
                {
                    // Debug.Log($"Grid Found: {list.Count}");
                    results.UnionWith(list);
                }
            }
        }
        //TODO 校验这个是否正确可靠，有没有什么方法可视化一下？
        public void QuerySegment(Vector3 start, Vector3 end, HashSet<T> results)
        {
            var startCell = GetCell(start);
            var endCell = GetCell(end);
            var revert = start.x < end.x;
            var startX = revert ? start.x : end.x;
            var startZ = revert ? start.z : end.z;
            var endZ = revert ? end.z : start.z;
            var sign = startZ < endZ ? 1 : -1;
            var len = Mathf.Abs(end.x - start.x) + 0.001f;
            var endIdxX = revert ? endCell.x : startCell.x;

            var idxX = revert ? startCell.x : endCell.x;
            var idxY = revert? startCell.y: endCell.y;
            while (true)
            {
                var checkX = _cellSize * idxX + _cellSize / 2;
                var percent = (checkX - startX) / len;
                var checkY = Mathf.Lerp(revert ? start.z : end.z, revert ? end.z : start.z, percent);
                while (true)
                {
                    var key = new Vector2Int(idxX, idxY);
                    if (_grid.TryGetValue(key, out var list)) results.UnionWith(list);
                    var pivotY = _cellSize * idxY;
                    var inRange = Mathf.Abs(pivotY - checkY) <= _cellSize / 2;
                    // Debug.Log(
                    // $"Add to List {key} ∈ {(revert ? startCell : endCell)}->{(revert ? endCell : startCell)}" +
                    // $"\t{pivotY}->{checkY}({(revert ? end.z : start.z)})={(Mathf.Abs(pivotY - checkY))}<{(_cellSize/2)}?{inRange}");
                    if (inRange) break;
                    idxY += sign;
                }
                idxX++;
                if (idxX > endIdxX) return;
            }
        }

        public bool TryGet(Vector2Int cell, out List<T> list)
        {
            return _grid.TryGetValue(cell, out list);
        }
        public List<T> this[Vector2Int cell]
        {
            get => _grid[cell];
            set => _grid[cell] = value;
        }
        public Dictionary<Vector2Int, List<T>> GetDatas() => _grid;
        public void Copy(SpatialHashGrid<T> src)
        {
            var datas = src.GetDatas();
            _grid.Clear();
            foreach (var pair in datas) _grid.Add(pair.Key, pair.Value);
        }
        public void Clear()
        {
            _grid.Clear();
        }
        public IEnumerator<T> GetEnumerator()
        {
            foreach(var pair in _grid)
            foreach (var item in pair.Value)
            {
                yield return item;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

