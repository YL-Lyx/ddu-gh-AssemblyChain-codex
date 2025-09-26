using System;
using System.Collections.Generic;
using AssemblyChain.Planning.Model;

namespace AssemblyChain.Geometry.Contact
{
    /// <summary>
    /// 接觸檢測選項和配置
    /// </summary>
    public record DetectionOptions(
        double Tolerance = 1e-4,
        double MinPatchArea = 0.0,
        double MinEdgeLength = 0.0,  // 過濾短邊
        int MaxParallelism = 4,      // 限制並發
        string BroadPhase = "SAP",   // 寬相位算法選擇
        bool UseFastBrep = true,     // 是否啟用加速的Brep檢測
        bool FallbackToLegacyBrep = false,
        bool FastBrepParallel = true,
        double FastBrepNormalQuantization = 0.02,
        double FastBrepOffsetQuantization = 1e-3
    );


    /// <summary>
    /// 接觸檢測常量
    /// </summary>
    public static class ContactDetectionConstants
    {
        // 默認容差
        public const double DefaultTolerance = 1e-4;
        public const double DefaultMinPatchArea = 0.0;
        public const double DefaultMinEdgeLength = 0.0;
        public const int DefaultMaxParallelism = 4;
        public const string DefaultBroadPhase = "SAP";

        // 自適應參數係數
        public const double AdaptiveTolFactor = 1e-6;
        public const double AdaptiveMinAreaFactor = 1e-8;
        public const double BoundingBoxInflateFactor = 2.0;

        // 共面檢測參數
        public const double CoplanarAngleTolerance = 10.0; // 度
        public const double CoplanarDistanceFactor = 5.0;

        // 去重參數
        public const double CentroidQuantizationFactor = 0.1;
        public const double AreaQuantizationFactor = 0.1;
        public const double PlaneQuantizationFactor = 0.1;
    }
}
