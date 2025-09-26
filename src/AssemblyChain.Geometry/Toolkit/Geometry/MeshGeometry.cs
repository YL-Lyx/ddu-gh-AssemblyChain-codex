using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using AssemblyChain.IO.Contracts;
using Math = System.Math;

namespace AssemblyChain.Geometry.Toolkit.Geometry
{
    /// <summary>
    /// 网格几何计算工具类 - 从MeshContactDetector.cs重构
    /// 提供网格相关的几何计算功能
    /// </summary>
    public static class MeshGeometry
    {
        /// <summary>
        /// 计算网格面的中心点
        /// </summary>
        /// <param name="mesh">网格</param>
        /// <param name="face">网格面</param>
        /// <returns>面的中心点</returns>
        public static Point3d CalculateFaceCenter(Rhino.Geometry.Mesh mesh, Rhino.Geometry.MeshFace face)
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

        /// <summary>
        /// 计算网格面的面积
        /// </summary>
        /// <param name="mesh">网格</param>
        /// <param name="face">网格面</param>
        /// <returns>面积</returns>
        public static double CalculateFaceArea(Rhino.Geometry.Mesh mesh, Rhino.Geometry.MeshFace face)
        {
            try
            {
                var vertices = new Point3d[] {
                    mesh.Vertices[face.A],
                    mesh.Vertices[face.B],
                    mesh.Vertices[face.C]
                };

                if (face.IsQuad)
                {
                    // 四边形拆分为两个三角形
                    var area1 = CalculateTriangleArea(vertices[0], vertices[1], vertices[2]);
                    var area2 = CalculateTriangleArea(vertices[0], vertices[2], mesh.Vertices[face.D]);
                    return area1 + area2;
                }
                else
                {
                    // 三角形
                    return CalculateTriangleArea(vertices[0], vertices[1], vertices[2]);
                }
            }
            catch
            {
                return 0.0;
            }
        }

        /// <summary>
        /// 计算三角形面积
        /// </summary>
        private static double CalculateTriangleArea(Point3d a, Point3d b, Point3d c)
        {
            var v1 = b - a;
            var v2 = c - a;
            var cross = Vector3d.CrossProduct(v1, v2);
            return cross.Length / 2.0;
        }

        /// <summary>
        /// 获取网格面的法线
        /// </summary>
        /// <param name="mesh">网格</param>
        /// <param name="faceIndex">面索引</param>
        /// <returns>法线向量</returns>
        public static Vector3d GetFaceNormal(Rhino.Geometry.Mesh mesh, int faceIndex)
        {
            if (faceIndex < 0 || faceIndex >= mesh.Faces.Count)
                return Vector3d.ZAxis;

            mesh.FaceNormals.ComputeFaceNormals();
            if (faceIndex < mesh.FaceNormals.Count)
                return mesh.FaceNormals[faceIndex];

            return Vector3d.ZAxis;
        }

        /// <summary>
        /// 计算两个网格之间的最小距离
        /// </summary>
        /// <param name="meshA">第一个网格</param>
        /// <param name="meshB">第二个网格</param>
        /// <param name="maxComparisons">最大比较次数</param>
        /// <returns>最小距离</returns>
        public static double CalculateMinDistance(Rhino.Geometry.Mesh meshA, Rhino.Geometry.Mesh meshB, int maxComparisons = 10000)
        {
            double minDistance = double.MaxValue;

            try
            {
                var comparisons = 0;
                var sampleStepA = System.Math.Max(1, meshA.Faces.Count / 100);
                var sampleStepB = System.Math.Max(1, meshB.Faces.Count / 100);

                for (int i = 0; i < meshA.Faces.Count && comparisons < maxComparisons; i += sampleStepA)
                {
                    var faceA = meshA.Faces[i];
                    var centerA = CalculateFaceCenter(meshA, faceA);

                    for (int j = 0; j < meshB.Faces.Count && comparisons < maxComparisons; j += sampleStepB)
                    {
                        var faceB = meshB.Faces[j];
                        var centerB = CalculateFaceCenter(meshB, faceB);

                        var distance = centerA.DistanceTo(centerB);
                        if (distance < minDistance)
                        {
                            minDistance = distance;

                            if (minDistance < 1e-10)
                                return minDistance;
                        }

                        comparisons++;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in distance calculation: {ex.Message}");
                minDistance = 0.0; // 假设有接触
            }

            return minDistance;
        }

        /// <summary>
        /// 近似计算多边形面积
        /// </summary>
        /// <param name="polyline">多边形</param>
        /// <returns>面积</returns>
        public static double ApproximateArea(Polyline polyline)
        {
            if (!polyline.IsClosed) return 0.0;

            double area = 0.0;
            var points = polyline.ToArray();

            for (int i = 0; i < points.Length - 1; i++)
            {
                area += points[i].X * points[i + 1].Y - points[i + 1].X * points[i].Y;
            }

            return System.Math.Abs(area) / 2.0;
        }

        /// <summary>
        /// 计算几何体的面积
        /// </summary>
        /// <param name="geometry">几何体</param>
        /// <returns>面积</returns>
        public static double ComputeGeometryArea(GeometryBase geometry)
        {
            try
            {
                if (geometry is Rhino.Geometry.Mesh mesh)
                {
                    var areaProps = AreaMassProperties.Compute(mesh);
                    return areaProps?.Area ?? 0.0;
                }
                else if (geometry is Curve curve)
                {
                    // 对于曲线，计算包围盒面积作为近似
                    var bbox = curve.GetBoundingBox(true);
                    return bbox.Area;
                }
                else if (geometry is Surface surface)
                {
                    var areaProps = AreaMassProperties.Compute(surface);
                    return areaProps?.Area ?? 0.0;
                }
            }
            catch
            {
                // 忽略错误
            }
            return 0.0;
        }

        /// <summary>
        /// 从几何推断接触平面
        /// </summary>
        /// <param name="geometry">几何体</param>
        /// <returns>接触平面</returns>
        public static ContactPlane InferContactPlane(GeometryBase geometry)
        {
            try
            {
                if (geometry is Rhino.Geometry.Mesh mesh && mesh.Faces.Count > 0)
                {
                    var face = mesh.Faces[0];
                    var normal = GetFaceNormal(mesh, 0);
                    var center = CalculateFaceCenter(mesh, face);

                    var plane = new Plane(center, new Vector3d(normal.X, normal.Y, normal.Z));
                    return new ContactPlane(plane, plane.Normal, plane.Origin);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inferring contact plane: {ex.Message}");
            }

            return new ContactPlane(Plane.WorldXY, Vector3d.ZAxis, Point3d.Origin);
        }
    }
}
