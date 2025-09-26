using System;
using System.Collections.Generic;
using Rhino.Geometry;
using Math = System.Math;

namespace AssemblyChain.Core.Toolkit.Mesh
{
    /// <summary>
    /// 网格空间索引 - 从MeshContactDetector.cs重构
    /// 用于高效的网格面查询和碰撞检测
    /// </summary>
    public class MeshSpatialIndex
    {
        private readonly Dictionary<int, List<int>> _spatialGrid;
        private readonly double _cellSize;
        private readonly BoundingBox _bounds;
        private readonly Rhino.Geometry.Mesh _mesh;

        /// <summary>
        /// 创建空间索引
        /// </summary>
        /// <param name="mesh">要索引的网格</param>
        /// <param name="cellSize">网格单元大小，如果为0则自动计算</param>
        public MeshSpatialIndex(Rhino.Geometry.Mesh mesh, double cellSize = 0)
        {
            _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));

            // 计算边界和单元大小
            _bounds = mesh.GetBoundingBox(true);
            _cellSize = cellSize > 0 ? cellSize : _bounds.Diagonal.Length * 0.05; // 默认5%的对角线长度

            _spatialGrid = new Dictionary<int, List<int>>();
            BuildIndex();
        }

        /// <summary>
        /// 构建空间索引
        /// </summary>
        private void BuildIndex()
        {
            for (int i = 0; i < _mesh.Faces.Count; i++)
            {
                var face = _mesh.Faces[i];
                var center = CalculateFaceCenter(_mesh, face);
                var cellKey = GetCellKey(center);

                if (!_spatialGrid.ContainsKey(cellKey))
                    _spatialGrid[cellKey] = new List<int>();

                _spatialGrid[cellKey].Add(i);
            }
        }

        /// <summary>
        /// 获取指定点附近的网格面
        /// </summary>
        /// <param name="queryPoint">查询点</param>
        /// <param name="radius">搜索半径</param>
        /// <returns>附近网格面的索引列表</returns>
        public IEnumerable<int> GetNearbyFaces(Point3d queryPoint, double radius)
        {
            var cells = GetNearbyCells(queryPoint, radius);
            var nearbyFaces = new HashSet<int>();

            foreach (var cellKey in cells)
            {
                if (_spatialGrid.ContainsKey(cellKey))
                {
                    foreach (var faceIndex in _spatialGrid[cellKey])
                        nearbyFaces.Add(faceIndex);
                }
            }

            return nearbyFaces;
        }

        /// <summary>
        /// 获取指定区域内的网格面
        /// </summary>
        /// <param name="region">查询区域</param>
        /// <returns>区域内的网格面索引列表</returns>
        public IEnumerable<int> GetFacesInRegion(BoundingBox region)
        {
            var faces = new HashSet<int>();

            // 计算区域覆盖的网格单元
            var minCell = GetCellKey(region.Min);
            var maxCell = GetCellKey(region.Max);

            // 简单的方法：遍历所有可能的单元
            // 实际应用中可以优化为只遍历相关单元
            for (int x = GetXFromKey(minCell); x <= GetXFromKey(maxCell); x++)
            {
                for (int y = GetYFromKey(minCell); y <= GetYFromKey(maxCell); y++)
                {
                    for (int z = GetZFromKey(minCell); z <= GetZFromKey(maxCell); z++)
                    {
                        var cellKey = GetCellKeyFromXYZ(x, y, z);
                        if (_spatialGrid.ContainsKey(cellKey))
                        {
                            foreach (var faceIndex in _spatialGrid[cellKey])
                            {
                                // 检查面是否真的在区域内
                                var face = _mesh.Faces[faceIndex];
                                var center = CalculateFaceCenter(_mesh, face);
                                if (region.Contains(center))
                                {
                                    faces.Add(faceIndex);
                                }
                            }
                        }
                    }
                }
            }

            return faces;
        }

        /// <summary>
        /// 获取索引统计信息
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                ["TotalCells"] = _spatialGrid.Count,
                ["CellSize"] = _cellSize,
                ["TotalFaces"] = _mesh.Faces.Count,
                ["AverageFacesPerCell"] = (double)_mesh.Faces.Count / System.Math.Max(1, _spatialGrid.Count),
                ["Bounds"] = _bounds
            };
        }

        /// <summary>
        /// 计算网格单元键
        /// </summary>
        private int GetCellKey(Point3d point)
        {
            int x = (int)((point.X - _bounds.Min.X) / _cellSize);
            int y = (int)((point.Y - _bounds.Min.Y) / _cellSize);
            int z = (int)((point.Z - _bounds.Min.Z) / _cellSize);

            // 简单的3D坐标哈希
            return x * 73856093 ^ y * 19349663 ^ z * 83492791;
        }

        /// <summary>
        /// 从单元键获取坐标
        /// </summary>
        private int GetXFromKey(int key) => (key / 73856093) & 0xFFFF;
        private int GetYFromKey(int key) => ((key / 19349663) & 0xFFFF);
        private int GetZFromKey(int key) => (key & 0xFFFF);

        /// <summary>
        /// 从XYZ坐标创建单元键
        /// </summary>
        private int GetCellKeyFromXYZ(int x, int y, int z)
        {
            return x * 73856093 ^ y * 19349663 ^ z * 83492791;
        }

        /// <summary>
        /// 获取附近的网格单元
        /// </summary>
        private IEnumerable<int> GetNearbyCells(Point3d center, double radius)
        {
            int radiusInCells = (int)System.Math.Ceiling(radius / _cellSize);
            var cells = new HashSet<int>();

            for (int dx = -radiusInCells; dx <= radiusInCells; dx++)
            {
                for (int dy = -radiusInCells; dy <= radiusInCells; dy++)
                {
                    for (int dz = -radiusInCells; dz <= radiusInCells; dz++)
                    {
                        var testPoint = new Point3d(
                            center.X + dx * _cellSize,
                            center.Y + dy * _cellSize,
                            center.Z + dz * _cellSize);
                        cells.Add(GetCellKey(testPoint));
                    }
                }
            }

            return cells;
        }

        /// <summary>
        /// 计算网格面的中心点
        /// </summary>
        private static Point3d CalculateFaceCenter(Rhino.Geometry.Mesh mesh, Rhino.Geometry.MeshFace face)
        {
            var vertices = new List<Point3d>
            {
                mesh.Vertices[face.A],
                mesh.Vertices[face.B],
                mesh.Vertices[face.C]
            };

            if (face.IsQuad)
                vertices.Add(mesh.Vertices[face.D]);

            var center = Point3d.Origin;
            foreach (var v in vertices)
                center += v;
            center /= vertices.Count;

            return center;
        }
    }
}
