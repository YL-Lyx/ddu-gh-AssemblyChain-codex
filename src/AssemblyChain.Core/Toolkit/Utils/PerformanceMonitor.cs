using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AssemblyChain.Core.Toolkit.Utils
{
    /// <summary>
    /// 性能监控和调试工具 - 从MeshContactDetector.cs重构
    /// 提供统一的性能监控接口
    /// </summary>
    public class PerformanceMonitor
    {
        private readonly Dictionary<string, (DateTime start, TimeSpan duration)> _timings;
        private readonly List<string> _debugLog;

        /// <summary>
        /// 创建性能监控器
        /// </summary>
        public PerformanceMonitor()
        {
            _timings = new Dictionary<string, (DateTime, TimeSpan)>();
            _debugLog = new List<string>();
        }

        /// <summary>
        /// 开始计时
        /// </summary>
        /// <param name="operation">操作名称</param>
        public void StartTimer(string operation)
        {
            _timings[operation] = (DateTime.Now, TimeSpan.Zero);
            LogDebug($"Started: {operation}");
        }

        /// <summary>
        /// 停止计时
        /// </summary>
        /// <param name="operation">操作名称</param>
        public void StopTimer(string operation)
        {
            if (_timings.ContainsKey(operation))
            {
                var start = _timings[operation].start;
                var duration = DateTime.Now - start;
                _timings[operation] = (start, duration);
                LogDebug($"Completed: {operation} in {duration.TotalMilliseconds:F2}ms");
            }
        }

        /// <summary>
        /// 获取操作耗时
        /// </summary>
        /// <param name="operation">操作名称</param>
        /// <returns>耗时（毫秒）</returns>
        public double GetDuration(string operation)
        {
            return _timings.ContainsKey(operation) ? _timings[operation].duration.TotalMilliseconds : 0;
        }

        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="message">调试消息</param>
        public void LogDebug(string message)
        {
            var timestamped = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
            _debugLog.Add(timestamped);
            Debug.WriteLine($"[PerformanceMonitor] {timestamped}");
        }

        /// <summary>
        /// 生成性能报告
        /// </summary>
        /// <returns>性能报告字符串</returns>
        public string GenerateReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Performance Report ===");

            var totalTime = _timings.Values.Sum(t => t.duration.TotalMilliseconds);
            sb.AppendLine($"Total Time: {totalTime:F2}ms");

            if (_timings.Count > 0)
            {
                sb.AppendLine("Operation Timings:");

                foreach (var timing in _timings.OrderByDescending(t => t.Value.duration.TotalMilliseconds))
                {
                    var percentage = totalTime > 0 ? (timing.Value.duration.TotalMilliseconds / totalTime * 100) : 0;
                    sb.AppendLine($"  {timing.Key}: {timing.Value.duration.TotalMilliseconds:F2}ms ({percentage:F1}%)");
                }
            }

            if (_debugLog.Count > 0)
            {
                sb.AppendLine("Recent Debug Log:");
                foreach (var log in _debugLog.TakeLast(10))
                {
                    sb.AppendLine($"  {log}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取性能统计
        /// </summary>
        /// <returns>性能统计字典</returns>
        public Dictionary<string, object> GetStatistics()
        {
            var totalTime = _timings.Values.Sum(t => t.duration.TotalMilliseconds);

            return new Dictionary<string, object>
            {
                ["TotalTime"] = totalTime,
                ["OperationCount"] = _timings.Count,
                ["LogEntries"] = _debugLog.Count,
                ["AverageTimePerOperation"] = _timings.Count > 0 ? totalTime / _timings.Count : 0,
                ["SlowestOperation"] = _timings.OrderByDescending(t => t.Value.duration.TotalMilliseconds)
                                              .FirstOrDefault().Key ?? "None",
                ["FastestOperation"] = _timings.OrderBy(t => t.Value.duration.TotalMilliseconds)
                                              .FirstOrDefault().Key ?? "None"
            };
        }

        /// <summary>
        /// 重置监控器
        /// </summary>
        public void Reset()
        {
            _timings.Clear();
            _debugLog.Clear();
        }

        /// <summary>
        /// 获取所有操作名称
        /// </summary>
        public IEnumerable<string> GetOperationNames()
        {
            return _timings.Keys;
        }

        /// <summary>
        /// 检查操作是否正在运行
        /// </summary>
        /// <param name="operation">操作名称</param>
        /// <returns>是否正在运行</returns>
        public bool IsRunning(string operation)
        {
            return _timings.ContainsKey(operation) && _timings[operation].duration == TimeSpan.Zero;
        }

        /// <summary>
        /// 获取运行中的操作
        /// </summary>
        /// <returns>运行中的操作名称列表</returns>
        public IEnumerable<string> GetRunningOperations()
        {
            return _timings.Where(t => t.Value.duration == TimeSpan.Zero).Select(t => t.Key);
        }
    }

    /// <summary>
    /// 性能监控辅助类 - 提供简化的使用接口
    /// </summary>
    public static class PerformanceMonitorHelper
    {
        private static readonly Dictionary<string, PerformanceMonitor> _monitors = new();

        /// <summary>
        /// 获取或创建监控器
        /// </summary>
        /// <param name="name">监控器名称</param>
        /// <returns>性能监控器实例</returns>
        public static PerformanceMonitor GetMonitor(string name)
        {
            if (!_monitors.ContainsKey(name))
            {
                _monitors[name] = new PerformanceMonitor();
            }
            return _monitors[name];
        }

        /// <summary>
        /// 移除监控器
        /// </summary>
        /// <param name="name">监控器名称</param>
        public static void RemoveMonitor(string name)
        {
            _monitors.Remove(name);
        }

        /// <summary>
        /// 清理所有监控器
        /// </summary>
        public static void ClearAll()
        {
            _monitors.Clear();
        }

        /// <summary>
        /// 生成所有监控器的综合报告
        /// </summary>
        /// <returns>综合性能报告</returns>
        public static string GenerateGlobalReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Global Performance Report ===");
            sb.AppendLine($"Active Monitors: {_monitors.Count}");

            foreach (var kvp in _monitors)
            {
                sb.AppendLine($"--- Monitor: {kvp.Key} ---");
                sb.AppendLine(kvp.Value.GenerateReport());
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
