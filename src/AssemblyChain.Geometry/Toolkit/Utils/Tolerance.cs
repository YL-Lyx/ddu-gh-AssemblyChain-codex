using System;
using System.Collections.Generic;

namespace AssemblyChain.Geometry.Toolkit.Utils
{
    /// <summary>
    /// Global tolerance strategies for geometric and boolean operations.
    /// </summary>
    public static class Tolerance
    {
        /// <summary>
        /// Tolerance settings for different operation types.
        /// </summary>
        public class ToleranceSettings
        {
            public double GeometricTolerance { get; set; } = 1e-6;
            public double BooleanTolerance { get; set; } = 1e-6;
            public double ProjectionTolerance { get; set; } = 1e-9;
            public double IntersectionTolerance { get; set; } = 1e-6;
            public double ContactTolerance { get; set; } = 1e-6;
            public double MeshTolerance { get; set; } = 1e-6;
            public double AngleToleranceDeg { get; set; } = 0.1;
            public double AreaTolerance { get; set; } = 1e-9;
            public double VolumeTolerance { get; set; } = 1e-12;
            public bool UseAdaptiveTolerance { get; set; } = true;
            public double AdaptiveScaleFactor { get; set; } = 1e-6;
        }

        private static ToleranceSettings _currentSettings = new ToleranceSettings();

        public static ToleranceSettings Current
        {
            get => _currentSettings;
            set => _currentSettings = value ?? new ToleranceSettings();
        }

        public static double GetAdaptiveTolerance(double objectSize)
        {
            if (!Current.UseAdaptiveTolerance) return Current.GeometricTolerance;
            return System.Math.Max(Current.GeometricTolerance, objectSize * Current.AdaptiveScaleFactor);
        }

        public static double GetAdaptiveTolerance(Rhino.Geometry.BoundingBox bbox)
        {
            if (!bbox.IsValid) return Current.GeometricTolerance;
            var diagonal = bbox.Diagonal.Length;
            return GetAdaptiveTolerance(diagonal);
        }

        public static bool Equal(double a, double b, double? customTolerance = null)
        {
            var tol = customTolerance ?? Current.GeometricTolerance;
            return System.Math.Abs(a - b) <= tol;
        }

        public static bool IsZero(double value, double? customTolerance = null)
        {
            var tol = customTolerance ?? Current.GeometricTolerance;
            return System.Math.Abs(value) <= tol;
        }

        public static bool PointsEqual(Rhino.Geometry.Point3d a, Rhino.Geometry.Point3d b, double? customTolerance = null)
        {
            var tol = customTolerance ?? Current.GeometricTolerance;
            return a.DistanceTo(b) <= tol;
        }

        public static bool VectorsParallel(Rhino.Geometry.Vector3d a, Rhino.Geometry.Vector3d b, double? customAngleTolDeg = null)
        {
            var angleTol = (customAngleTolDeg ?? Current.AngleToleranceDeg) * System.Math.PI / 180.0;
            if (a.IsZero || b.IsZero) return true;
            var angle = Rhino.Geometry.Vector3d.VectorAngle(a, b);
            return angle <= angleTol || System.Math.PI - angle <= angleTol;
        }

        public static bool PlanesCoplanar(Rhino.Geometry.Plane a, Rhino.Geometry.Plane b, double? customTolerance = null)
        {
            var tol = customTolerance ?? Current.GeometricTolerance;
            if (!VectorsParallel(a.Normal, b.Normal)) return false;
            var originDiff = a.Origin - b.Origin;
            var distance = System.Math.Abs(Rhino.Geometry.Vector3d.Multiply(originDiff, a.Normal));
            return distance <= tol;
        }

        public static bool IsSignificantArea(double area)
        {
            return area >= Current.AreaTolerance;
        }

        public static bool IsSignificantVolume(double volume)
        {
            return volume >= Current.VolumeTolerance;
        }

        public static double RoundToTolerance(double value, double? customTolerance = null)
        {
            var tol = customTolerance ?? Current.GeometricTolerance;
            return System.Math.Round(value / tol) * tol;
        }

        public class ToleranceContext : IDisposable
        {
            private readonly ToleranceSettings _originalSettings;
            public ToleranceContext(ToleranceSettings newSettings)
            {
                _originalSettings = Current;
                Current = newSettings;
            }
            public void Dispose()
            {
                Current = _originalSettings;
            }
        }

        public static ToleranceContext CreateContext(ToleranceSettings settings)
        {
            return new ToleranceContext(settings);
        }

        public static ToleranceContext CreateRobustContext(double scaleFactor = 10.0)
        {
            var robustSettings = new ToleranceSettings
            {
                GeometricTolerance = Current.GeometricTolerance * scaleFactor,
                BooleanTolerance = Current.BooleanTolerance * scaleFactor,
                ProjectionTolerance = Current.ProjectionTolerance * scaleFactor,
                IntersectionTolerance = Current.IntersectionTolerance * scaleFactor,
                ContactTolerance = Current.ContactTolerance * scaleFactor,
                MeshTolerance = Current.MeshTolerance * scaleFactor,
                AngleToleranceDeg = Current.AngleToleranceDeg * scaleFactor,
                AreaTolerance = Current.AreaTolerance * scaleFactor,
                VolumeTolerance = Current.VolumeTolerance * scaleFactor,
                UseAdaptiveTolerance = Current.UseAdaptiveTolerance,
                AdaptiveScaleFactor = Current.AdaptiveScaleFactor * scaleFactor
            };
            return new ToleranceContext(robustSettings);
        }

        public static bool ValidateSettings(ToleranceSettings settings, out List<string> issues)
        {
            issues = new List<string>();
            if (settings == null)
            {
                issues.Add("Settings cannot be null");
                return false;
            }
            if (settings.GeometricTolerance <= 0) issues.Add("GeometricTolerance must be positive");
            if (settings.BooleanTolerance <= 0) issues.Add("BooleanTolerance must be positive");
            if (settings.ProjectionTolerance <= 0) issues.Add("ProjectionTolerance must be positive");
            if (settings.IntersectionTolerance <= 0) issues.Add("IntersectionTolerance must be positive");
            if (settings.ContactTolerance <= 0) issues.Add("ContactTolerance must be positive");
            if (settings.MeshTolerance <= 0) issues.Add("MeshTolerance must be positive");
            if (settings.AngleToleranceDeg <= 0) issues.Add("AngleToleranceDeg must be positive");
            if (settings.AreaTolerance <= 0) issues.Add("AreaTolerance must be positive");
            if (settings.VolumeTolerance <= 0) issues.Add("VolumeTolerance must be positive");
            if (settings.AdaptiveScaleFactor <= 0) issues.Add("AdaptiveScaleFactor must be positive");
            return issues.Count == 0;
        }

        public static string GetDescription()
        {
            return $"Geometric: {Current.GeometricTolerance:F2e}, " +
                   $"Boolean: {Current.BooleanTolerance:F2e}, " +
                   $"Angle: {Current.AngleToleranceDeg:F3}°, " +
                   $"Adaptive: {(Current.UseAdaptiveTolerance ? "ON" : "OFF")}";
        }
    }
}



