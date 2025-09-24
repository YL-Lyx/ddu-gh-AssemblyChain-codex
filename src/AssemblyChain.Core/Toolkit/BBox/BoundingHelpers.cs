using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

namespace AssemblyChain.Core.Toolkit.BBox
{
	/// <summary>
	/// Utilities for bounding box operations including expansion, intersection, and voxel approximation.
	/// </summary>
	public static class BoundingHelpers
	{
		/// <summary>
		/// Bounding box expansion options.
		/// </summary>
		public class ExpansionOptions
		{
			public double UniformExpansion { get; set; } = 0.0;
			public double XExpansion { get; set; } = 0.0;
			public double YExpansion { get; set; } = 0.0;
			public double ZExpansion { get; set; } = 0.0;
			public bool SymmetricExpansion { get; set; } = true;
			public double Tolerance { get; set; } = 1e-6;
		}

		/// <summary>
		/// Voxelization options.
		/// </summary>
		public class VoxelOptions
		{
			public double VoxelSize { get; set; } = 1.0;
			public bool FillInterior { get; set; } = false;
			public bool IncludeBoundary { get; set; } = true;
			public int MaxVoxels { get; set; } = 10000;
		}

		/// <summary>
		/// Expands a bounding box by specified amounts.
		/// </summary>
		public static BoundingBox ExpandBoundingBox(BoundingBox bbox, ExpansionOptions options)
		{
			if (!bbox.IsValid) return bbox;
			options ??= new ExpansionOptions();

			var min = bbox.Min;
			var max = bbox.Max;

			if (options.SymmetricExpansion)
			{
				// Symmetric expansion
				double expandX = options.UniformExpansion + options.XExpansion;
				double expandY = options.UniformExpansion + options.YExpansion;
				double expandZ = options.UniformExpansion + options.ZExpansion;

				min.X -= expandX;
				min.Y -= expandY;
				min.Z -= expandZ;
				max.X += expandX;
				max.Y += expandY;
				max.Z += expandZ;
			}
			else
			{
				// Asymmetric expansion (only positive direction)
				max.X += options.UniformExpansion + options.XExpansion;
				max.Y += options.UniformExpansion + options.YExpansion;
				max.Z += options.UniformExpansion + options.ZExpansion;
			}

			return new BoundingBox(min, max);
		}

		/// <summary>
		/// Computes the intersection of multiple bounding boxes.
		/// </summary>
		public static BoundingBox IntersectBoundingBoxes(IEnumerable<BoundingBox> bboxes)
		{
			if (bboxes == null) return BoundingBox.Empty;
			BoundingBox result = BoundingBox.Empty;
			bool initialized = false;

			foreach (var bbox in bboxes)
			{
				if (!bbox.IsValid) continue;
				if (!initialized)
				{
					result = bbox;
					initialized = true;
				}
				else
				{
					result = BoundingBox.Intersection(result, bbox);
				}
			}

			return result;
		}

		/// <summary>
		/// Computes the union of multiple bounding boxes.
		/// </summary>
		public static BoundingBox UnionBoundingBoxes(IEnumerable<BoundingBox> bboxes)
		{
			if (bboxes == null) return BoundingBox.Empty;
			BoundingBox result = BoundingBox.Empty;
			bool initialized = false;

			foreach (var bbox in bboxes)
			{
				if (!bbox.IsValid) continue;
				if (!initialized)
				{
					result = bbox;
					initialized = true;
				}
				else
				{
					result.Union(bbox);
				}
			}

			return result;
		}

		/// <summary>
		/// Checks if two bounding boxes intersect.
		/// </summary>
		public static bool BoundingBoxesIntersect(BoundingBox bbox1, BoundingBox bbox2, double tolerance = 1e-6)
		{
			if (!bbox1.IsValid || !bbox2.IsValid) return false;

			return !(bbox1.Max.X + tolerance < bbox2.Min.X ||
					bbox2.Max.X + tolerance < bbox1.Min.X ||
					bbox1.Max.Y + tolerance < bbox2.Min.Y ||
					bbox2.Max.Y + tolerance < bbox1.Min.Y ||
					bbox1.Max.Z + tolerance < bbox2.Min.Z ||
					bbox2.Max.Z + tolerance < bbox1.Min.Z);
		}

		/// <summary>
		/// Computes the surface area of a bounding box.
		/// </summary>
		public static double BoundingBoxSurfaceArea(BoundingBox bbox)
		{
			if (!bbox.IsValid) return 0.0;
			var size = bbox.Diagonal;
			return 2.0 * (size.X * size.Y + size.Y * size.Z + size.Z * size.X);
		}

		/// <summary>
		/// Computes the volume of a bounding box.
		/// </summary>
		public static double BoundingBoxVolume(BoundingBox bbox)
		{
			if (!bbox.IsValid) return 0.0;
			var size = bbox.Diagonal;
			return size.X * size.Y * size.Z;
		}

		/// <summary>
		/// Creates a voxel approximation of a bounding box.
		/// </summary>
		public static List<Point3d> VoxelizeBoundingBox(BoundingBox bbox, VoxelOptions options)
		{
			var voxels = new List<Point3d>();
			if (!bbox.IsValid) return voxels;
			options ??= new VoxelOptions();

			double voxelSize = System.Math.Max(options.VoxelSize, 1e-9);
			var min = bbox.Min;
			var max = bbox.Max;

			int gridSizeX = (int)System.Math.Ceiling((max.X - min.X) / voxelSize);
			int gridSizeY = (int)System.Math.Ceiling((max.Y - min.Y) / voxelSize);
			int gridSizeZ = (int)System.Math.Ceiling((max.Z - min.Z) / voxelSize);

			long totalVoxels = (long)gridSizeX * gridSizeY * gridSizeZ;
			if (totalVoxels > options.MaxVoxels)
			{
				return voxels; // too many voxels, bail out
			}

			for (int x = 0; x < gridSizeX; x++)
			{
				for (int y = 0; y < gridSizeY; y++)
				{
					for (int z = 0; z < gridSizeZ; z++)
					{
						var voxelCenter = new Point3d(
							min.X + (x + 0.5) * voxelSize,
							min.Y + (y + 0.5) * voxelSize,
							min.Z + (z + 0.5) * voxelSize
						);

						bool isOnBoundary = IsOnBoundary(voxelCenter, bbox, voxelSize * 0.5 + 1e-9);
						if ((isOnBoundary && options.IncludeBoundary) || (!isOnBoundary && options.FillInterior))
						{
							voxels.Add(voxelCenter);
						}
					}
				}
			}

			return voxels;
		}

		/// <summary>
		/// Checks if a point is on the boundary of a bounding box.
		/// </summary>
		private static bool IsOnBoundary(Point3d point, BoundingBox bbox, double tolerance)
		{
			var min = bbox.Min;
			var max = bbox.Max;
			return System.Math.Abs(point.X - min.X) < tolerance || System.Math.Abs(point.X - max.X) < tolerance ||
				   System.Math.Abs(point.Y - min.Y) < tolerance || System.Math.Abs(point.Y - max.Y) < tolerance ||
				   System.Math.Abs(point.Z - min.Z) < tolerance || System.Math.Abs(point.Z - max.Z) < tolerance;
		}

		/// <summary>
		/// Creates a minimal bounding box that contains all input points.
		/// </summary>
		public static BoundingBox CreateBoundingBox(IEnumerable<Point3d> points)
		{
			var bbox = BoundingBox.Empty;
			if (points == null) return bbox;
			foreach (var point in points)
			{
				bbox.Union(point);
			}
			return bbox;
		}

		/// <summary>
		/// Computes the center of a bounding box.
		/// </summary>
		public static Point3d BoundingBoxCenter(BoundingBox bbox)
		{
			if (!bbox.IsValid) return Point3d.Unset;
			return new Point3d(
				(bbox.Min.X + bbox.Max.X) / 2.0,
				(bbox.Min.Y + bbox.Max.Y) / 2.0,
				(bbox.Min.Z + bbox.Max.Z) / 2.0
			);
		}

		/// <summary>
		/// Computes the diagonal vector of a bounding box.
		/// </summary>
		public static Vector3d BoundingBoxSize(BoundingBox bbox)
		{
			if (!bbox.IsValid) return Vector3d.Unset;
			return new Vector3d(
				bbox.Max.X - bbox.Min.X,
				bbox.Max.Y - bbox.Min.Y,
				bbox.Max.Z - bbox.Min.Z
			);
		}

		/// <summary>
		/// Checks if a point is contained within a bounding box.
		/// </summary>
		public static bool ContainsPoint(BoundingBox bbox, Point3d point, double tolerance = 1e-6)
		{
			if (!bbox.IsValid) return false;
			return point.X >= bbox.Min.X - tolerance && point.X <= bbox.Max.X + tolerance &&
				   point.Y >= bbox.Min.Y - tolerance && point.Y <= bbox.Max.Y + tolerance &&
				   point.Z >= bbox.Min.Z - tolerance && point.Z <= bbox.Max.Z + tolerance;
		}

		/// <summary>
		/// Computes the aspect ratio of a bounding box (longest to shortest dimension).
		/// </summary>
		public static double BoundingBoxAspectRatio(BoundingBox bbox)
		{
			if (!bbox.IsValid) return 1.0;
			var size = BoundingBoxSize(bbox);
			var dimensions = new[] { size.X, size.Y, size.Z }.Where(d => d > 0).ToArray();
			if (dimensions.Length < 2) return 1.0;
			double max = dimensions.Max();
			double min = dimensions.Min();
			return min > 0 ? max / min : double.PositiveInfinity;
		}
	}
}



