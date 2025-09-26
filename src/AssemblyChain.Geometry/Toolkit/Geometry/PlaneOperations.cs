using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;
using Math = System.Math;

namespace AssemblyChain.Geometry.Toolkit.Geometry
{
    /// <summary>
    /// 平面操作工具类 - 从MeshContactDetector.cs重构
    /// 提供平面相关的几何操作
    /// </summary>
    public static class PlaneOperations
    {
        /// <summary>
        /// 按平面分组网格面
        /// </summary>
        /// <param name="mesh">网格</param>
        /// <param name="tolerance">容差</param>
        /// <returns>平面到面索引列表的映射</returns>
        public static Dictionary<Plane, List<int>> GroupFacesByPlanes(Rhino.Geometry.Mesh mesh, double tolerance)
        {
            var planeGroups = new Dictionary<Plane, List<int>>();

            mesh.FaceNormals.ComputeFaceNormals();

            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                var face = mesh.Faces[i];
                var normal = mesh.FaceNormals[i];
                var center = MeshGeometry.CalculateFaceCenter(mesh, face);

                var plane = new Plane(center, new Vector3d(normal.X, normal.Y, normal.Z));

                var existingPlane = planeGroups.Keys.FirstOrDefault(p => ArePlanesCoplanar(p, plane, tolerance));

                if (existingPlane.IsValid)
                {
                    planeGroups[existingPlane].Add(i);
                }
                else
                {
                    planeGroups[plane] = new List<int> { i };
                }
            }

            return planeGroups;
        }

        /// <summary>
        /// 检查两个平面是否共面
        /// </summary>
        /// <param name="plane1">第一个平面</param>
        /// <param name="plane2">第二个平面</param>
        /// <param name="tolerance">容差</param>
        /// <returns>是否共面</returns>
        public static bool ArePlanesCoplanar(Plane plane1, Plane plane2, double tolerance)
        {
            var dot = System.Math.Abs(plane1.Normal * plane2.Normal);
            if (System.Math.Abs(dot - 1.0) > tolerance)
                return false;

            var distance = System.Math.Abs(plane1.DistanceTo(plane2.Origin));
            return distance <= tolerance;
        }

        /// <summary>
        /// 从曲线拟合平面
        /// </summary>
        /// <param name="curves">曲线列表</param>
        /// <param name="defaultNormal">默认法线</param>
        /// <returns>拟合的平面</returns>
        public static Plane FitPlaneFromCurves(List<Curve> curves, Vector3d defaultNormal)
        {
            var points = new List<Point3d>();

            foreach (var curve in curves)
            {
                var domain = curve.Domain;
                var samples = System.Math.Min(10, (int)domain.Length); // 根据曲线长度调整采样点数

                for (int i = 0; i <= samples; i++)
                {
                    var t = domain.ParameterAt((double)i / samples);
                    points.Add(curve.PointAt(t));
                }
            }

            if (points.Count < 3)
                return new Plane(points.FirstOrDefault(), defaultNormal);

            var result = Plane.FitPlaneToPoints(points, out var plane);
            return result == PlaneFitResult.Success
                ? plane
                : new Plane(points[0], defaultNormal);
        }

        /// <summary>
        /// 检查两个面是否共面且足够接近
        /// </summary>
        /// <param name="planeA">第一个面的平面</param>
        /// <param name="planeB">第二个面的平面</param>
        /// <param name="tolerance">容差</param>
        /// <returns>是否共面且接近</returns>
        public static bool AreFacesCoplanarAndClose(Plane planeA, Plane planeB, double tolerance)
        {
            // 检查法线是否平行
            var normalA = planeA.Normal;
            var normalB = planeB.Normal;

            if (normalA.IsParallelTo(normalB, tolerance) == 0) // 0表示不平行
                return false;

            // 检查两个面是否在同一平面内（距离检查）
            var distance = planeA.DistanceTo(planeB.Origin);
            return System.Math.Abs(distance) <= tolerance;
        }

        /// <summary>
        /// 获取网格面的平面
        /// </summary>
        /// <param name="mesh">网格</param>
        /// <param name="face">网格面</param>
        /// <returns>面的平面</returns>
        public static Plane GetFacePlane(Rhino.Geometry.Mesh mesh, Rhino.Geometry.MeshFace face)
        {
            var center = MeshGeometry.CalculateFaceCenter(mesh, face);

            // 找到face在mesh中的索引
            int faceIndex = -1;
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                if (mesh.Faces[i].A == face.A && mesh.Faces[i].B == face.B &&
                    mesh.Faces[i].C == face.C &&
                    (face.IsQuad ? mesh.Faces[i].D == face.D : true))
                {
                    faceIndex = i;
                    break;
                }
            }

            if (faceIndex >= 0 && faceIndex < mesh.FaceNormals.Count)
            {
                var normal = mesh.FaceNormals[faceIndex];
                return new Plane(center, new Vector3d(normal.X, normal.Y, normal.Z));
            }
            else
            {
                // 如果找不到对应的normal，使用默认法线
                return new Plane(center, Vector3d.ZAxis);
            }
        }

        /// <summary>
        /// 计算两个面的重叠区域
        /// </summary>
        /// <param name="meshA">第一个网格</param>
        /// <param name="faceA">第一个面</param>
        /// <param name="meshB">第二个网格</param>
        /// <param name="faceB">第二个面</param>
        /// <param name="plane">共享平面</param>
        /// <param name="tolerance">容差</param>
        /// <returns>重叠几何体</returns>
        public static GeometryBase ComputeFaceIntersectionGeometry(
            Rhino.Geometry.Mesh meshA, Rhino.Geometry.MeshFace faceA,
            Rhino.Geometry.Mesh meshB, Rhino.Geometry.MeshFace faceB,
            Plane plane, double tolerance)
        {
            try
            {
                // 获取面的顶点
                var verticesA = GetFaceVertices(meshA, faceA);
                var verticesB = GetFaceVertices(meshB, faceB);

                if (verticesA.Length < 3 || verticesB.Length < 3)
                    return null;

                // 创建多边形
                var polyA = new Polyline(verticesA);
                if (!polyA.IsClosed) polyA.Add(polyA[0]);

                var polyB = new Polyline(verticesB);
                if (!polyB.IsClosed) polyB.Add(polyB[0]);

                // 投影到共享平面并计算交集
                var projectedA = ProjectPolylineToPlane(polyA, plane);
                var projectedB = ProjectPolylineToPlane(polyB, plane);

                if (projectedA == null || projectedB == null)
                    return null;

                // 计算2D交集（简化实现 - 使用包围盒交集）
                var bboxA = projectedA.BoundingBox;
                var bboxB = projectedB.BoundingBox;
                var overlap2D = BoundingBox.Intersection(bboxA, bboxB);

                if (!overlap2D.IsValid)
                    return null;

                // 将2D交集转换回3D几何
                var center = overlap2D.Center;
                var size = overlap2D.Diagonal;

                // 创建代表性几何（矩形近似）
                var rect = new Rectangle3d(plane,
                    new Interval(center.X - size.X * 0.5, center.X + size.X * 0.5),
                    new Interval(center.Y - size.Y * 0.5, center.Y + size.Y * 0.5));

                var polyline = rect.ToPolyline();
                var mesh = Rhino.Geometry.Mesh.CreateFromClosedPolyline(polyline);

                return mesh != null && mesh.IsValid ? mesh : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取面的顶点
        /// </summary>
        private static Point3d[] GetFaceVertices(Rhino.Geometry.Mesh mesh, Rhino.Geometry.MeshFace face)
        {
            return new Point3d[] {
                mesh.Vertices[face.A],
                mesh.Vertices[face.B],
                mesh.Vertices[face.C],
                face.IsQuad ? mesh.Vertices[face.D] : mesh.Vertices[face.C] // 三角形重复最后一个顶点
            };
        }

        /// <summary>
        /// 将多边形投影到平面
        /// </summary>
        /// <param name="polyline">多边形</param>
        /// <param name="plane">目标平面</param>
        /// <returns>投影后的多边形</returns>
        public static Polyline ProjectPolylineToPlane(Polyline polyline, Plane plane)
        {
            var projectedPoints = new List<Point3d>();

            foreach (var point in polyline)
            {
                var projected = plane.ClosestPoint(point);
                projectedPoints.Add(projected);
            }

            return new Polyline(projectedPoints);
        }

        /// <summary>
        /// 计算两个2D多边形的交集
        /// </summary>
        /// <param name="polyA">第一个多边形</param>
        /// <param name="polyB">第二个多边形</param>
        /// <param name="tolerance">容差</param>
        /// <returns>交集多边形</returns>
        public static Polyline ComputePolygonIntersection2D(Polyline polyA, Polyline polyB, double tolerance)
        {
            // 简化实现：使用包围盒交集
            // 实际应用中应使用更精确的多边形裁剪算法（如Clipper库）
            var bboxA = polyA.BoundingBox;
            var bboxB = polyB.BoundingBox;

            var intersection = BoundingBox.Intersection(bboxA, bboxB);
            if (!intersection.IsValid)
                return null;

            // 从交集创建矩形
            var corners = new List<Point3d>
            {
                intersection.Corner(true, true, true),
                new Point3d(intersection.Max.X, intersection.Min.Y, 0),
                intersection.Corner(false, false, true),
                new Point3d(intersection.Min.X, intersection.Max.Y, 0),
                intersection.Corner(true, true, true) // 闭合
            };

            return new Polyline(corners);
        }

        /// <summary>
        /// 将2D多边形转换回平面上的3D
        /// </summary>
        /// <param name="poly2d">2D多边形</param>
        /// <param name="plane">目标平面</param>
        /// <returns>3D多边形</returns>
        public static Polyline ConvertPolygon2DTo3D(Polyline poly2d, Plane plane)
        {
            var points3d = poly2d.Select(p2d => plane.PointAt(p2d.X, p2d.Y)).ToList();
            return new Polyline(points3d);
        }

        /// <summary>
        /// 寻找距离最近的面对
        /// </summary>
        /// <param name="meshA">第一个网格</param>
        /// <param name="meshB">第二个网格</param>
        /// <param name="maxDistance">最大距离</param>
        /// <returns>最近的面对列表</returns>
        public static List<(Rhino.Geometry.MeshFace faceA, Rhino.Geometry.MeshFace faceB, double distance)> FindClosestFaces(
            Rhino.Geometry.Mesh meshA, Rhino.Geometry.Mesh meshB, double maxDistance)
        {
            var closestFaces = new List<(Rhino.Geometry.MeshFace, Rhino.Geometry.MeshFace, double)>();

            try
            {
                // 限制比较数量以提高性能
                int maxFaces = System.Math.Min(50, System.Math.Min(meshA.Faces.Count, meshB.Faces.Count));

                for (int i = 0; i < maxFaces; i++)
                {
                    var faceA = meshA.Faces[i];
                    var centerA = MeshGeometry.CalculateFaceCenter(meshA, faceA);

                    for (int j = 0; j < maxFaces; j++)
                    {
                        var faceB = meshB.Faces[j];
                        var centerB = MeshGeometry.CalculateFaceCenter(meshB, faceB);

                        var distance = centerA.DistanceTo(centerB);
                        if (distance <= maxDistance)
                        {
                            closestFaces.Add((faceA, faceB, distance));
                        }
                    }
                }

                // 按距离排序
                closestFaces.Sort((a, b) => a.Item3.CompareTo(b.Item3));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finding closest faces: {ex.Message}");
            }

            return closestFaces;
        }
    }
}
