//This is a c# port of the Javascript Polylabel module, which can be found here:
//https://github.com/mapbox/polylabel
//All credit goes to the original author

using UnityEngine;
using System.Collections.Generic;

namespace Nettle {
    public static class Polylabel {

        private class CellQueue {
            private List<Cell> _cells = new List<Cell>();
            public void Push(Cell cell) {
                if (_cells.Count == 0 || _cells[_cells.Count - 1].max > cell.max) {
                    _cells.Add(cell);
                    return;
                }
                if (_cells[0].max <= cell.max) {
                    _cells.Insert(0, cell);
                    return;
                }
                for (int i = 0; i < _cells.Count - 1; i++) {
                    if (_cells[i].max <= cell.max && _cells[i + 1].max > cell.max) {
                        _cells.Insert(i + 1, cell);
                        return;
                    }
                }
            }

            public int Count {
                get {
                    return _cells.Count;
                }
            }

            public Cell Pop() {
                Cell result = null;
                if (_cells.Count > 0) {
                    result = _cells[0];
                    _cells.RemoveAt(0);
                }
                return result;
            }
        }


        private class Cell {
            public float x {
                get {
                    return center.x;
                }
            }
            public float y {
                get {
                    return center.y;
                }
            }
            public Vector2 center;//cell center position
            public float h; // half the cell size
            public float d; // distance from cell center to polygon
            public float max; // max distance to polygon within a cell
            public static int CompareMax(Cell a, Cell b) {
                return (int)Mathf.Sign(b.max - a.max);
            }

            public Cell(Vector2 position, float h, Vector2[] polygon) {
                this.center = position;
                this.h = h;
                d = Polylabel.PointToPolygonDist(position, polygon);
                max = this.d + this.h * _sqrt2;
            }
        }

        private static Queue<Vector2> _queue = new Queue<Vector2>();
        private static readonly float _sqrt2 = Mathf.Sqrt(2);
        private static float _precision = 0.01f;
        public static float Precision {
            get {
                return _precision;
            }
            set {
                if (value > 0) {
                    _precision = value;
                }
            }
        }

        public static Vector3 FindPolygonCenter(Vector3[] polygon, bool zIsUp)
        {
            Vector2[] polygon2 = new Vector2[polygon.Length];
            for (int i = 0; i < polygon.Length; i++)
            {
                if (zIsUp)
                {
                    polygon2[i] = new Vector2(polygon[i].x, polygon[i].z);
                }else
                {
                    polygon2[i] = polygon[i];
                }
            }
            Vector2 result = FindPolygonCenter(polygon2);
            if (zIsUp)
            {
                return new Vector3(result.x, 0, result.y);
            }else
            {
                return result;
            }
        }

        public static Vector2 FindPolygonCenter(List<Vector2> polygon)
        {
            return FindPolygonCenter(polygon.ToArray());
        }

        public static Vector2 FindPolygonCenter(Vector2[] polygon) {
            if (polygon == null || polygon.Length == 0) {
                return Vector2.zero;
            }

            // find the bounding box of the outer ring
            float minX = polygon[0].x, minY = polygon[0].y, maxX = polygon[0].x, maxY = polygon[0].y;
            foreach (Vector2 p in polygon) {
                if (p.x < minX) minX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.x > maxX) maxX = p.x;
                if (p.y > maxY) maxY = p.y;
            }

            float width = maxX - minX;
            float height = maxY - minY;
            float cellSize = Mathf.Min(width, height);
            float h = cellSize / 2;

            if (cellSize == 0) return new Vector2(minX, minY);

            // a priority queue of cells in order of their "potential" (max distance to polygon)
            var cellQueue = new CellQueue();

            // cover polygon with initial cells
            for (var x = minX; x < maxX; x += cellSize) {
                for (var y = minY; y < maxY; y += cellSize) {
                    cellQueue.Push(new Cell(new Vector2(x + h, y + h), h, polygon));
                }
            }

            // take centroid as the first best guess
            Cell bestCell = getCentroidCell(polygon);

            // special case for rectangular polygons
            Cell bboxCell = new Cell(new Vector2(minX + width / 2, minY + height / 2), 0, polygon);
            if (bboxCell.d > bestCell.d) bestCell = bboxCell;

            int numProbes = cellQueue.Count;

            while (cellQueue.Count > 0) {
                // pick the most promising cell from the queue
                Cell cell = cellQueue.Pop();

                // update the best cell if we found a better one
                if (cell.d > bestCell.d) {
                    bestCell = cell;
                    //Debug.Log('found best %d after %d probes', Mathf.Round(1e4 * cell.d) / 1e4, numProbes);
                }

                // do not drill down further if there's no chance of a better solution
                if (cell.max - bestCell.d <= _precision) continue;

                // split the cell into four cells
                h = cell.h / 2;
                cellQueue.Push(new Cell(new Vector2(cell.x - h, cell.y - h), h, polygon));
                cellQueue.Push(new Cell(new Vector2(cell.x + h, cell.y - h), h, polygon));
                cellQueue.Push(new Cell(new Vector2(cell.x - h, cell.y + h), h, polygon));
                cellQueue.Push(new Cell(new Vector2(cell.x + h, cell.y + h), h, polygon));
                numProbes += 4;
            }

            /*
                Debug.log('num probes: ' + numProbes);
                Debug.log('best distance: ' + bestCell.d);
            */

            return bestCell.center;
        }

        // signed distance from point to polygon outline (negative if point is outside)
        private static float PointToPolygonDist(Vector2 position, Vector2[] polygon) {
            bool inside = false;
            float minDistSq = Mathf.Infinity;
            int length = polygon.Length;
            for (int i = 0, j = length - 1; i < length; j = i++) {
                var a = polygon[i];
                var b = polygon[j];

                if ((a[1] > position.y != b[1] > position.y) &&
                    (position.x < (b[0] - a[0]) * (position.y - a[1]) / (b[1] - a[1]) + a[0])) inside = !inside;

                minDistSq = Mathf.Min(minDistSq, GetSquaredDistanceToSegment(position, a, b));
            }


            return (inside ? 1 : -1) * Mathf.Sqrt(minDistSq);
        }

        // get polygon centroid
        private static Cell getCentroidCell(Vector2[] polygon) {
            float area = 0;
            float x = 0;
            float y = 0;
            int length = polygon.Length;
            for (int i = 0, j = length - 1; i < length; j = i++) {
                Vector2 a = polygon[i];
                Vector2 b = polygon[j];
                float f = a.x * b.y - b.x * a.y;
                x += (a.x + b.x) * f;
                y += (a.y + b.y) * f;
                area += f * 3;
            }
            if (area == 0) return new Cell(polygon[0], 0, polygon);
            return new Cell(new Vector2(x / area, y / area), 0, polygon);
        }

        // get squared distance from a point to a segment
        private static float GetSquaredDistanceToSegment(Vector2 point, Vector2 a, Vector2 b) {

            var x = a.x;
            var y = a.y;
            var dx = b.x - x;
            var dy = b.y - y;

            if (dx != 0 || dy != 0) {

                var t = ((point.x - x) * dx + (point.y - y) * dy) / (dx * dx + dy * dy);

                if (t > 1) {
                    x = b[0];
                    y = b[1];

                }
                else if (t > 0) {
                    x += dx * t;
                    y += dy * t;
                }
            }

            dx = point.x - x;
            dy = point.y - y;

            return dx * dx + dy * dy;
        }
    }

}
