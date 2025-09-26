using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace AssemblyChain.Geometry.Toolkit.Utils
{
    /// <summary>
    /// 並行處理工具類，提供高效的並行計算支持
    /// </summary>
    public static class ParallelProcessor
    {
        /// <summary>
        /// 並行處理列表中的項目
        /// </summary>
        public static List<R> ProcessInParallel<T, R>(
            IEnumerable<T> items,
            Func<T, R> processor,
            int? maxDegreeOfParallelism = null)
        {
            if (items == null) return new List<R>();
            var itemList = items.ToList();
            var results = new R[itemList.Count];
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism ?? Environment.ProcessorCount };

            System.Threading.Tasks.Parallel.For(0, itemList.Count, parallelOptions, i =>
            {
                try
                {
                    results[i] = processor(itemList[i]);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Parallel processing error: {ex.Message}");
                }
            });

            return results.Where(r => !(r is null)).ToList();
        }

        /// <summary>
        /// 並行處理列表中的項目（異步版本）
        /// </summary>
        public static async Task<List<R>> ProcessInParallelAsync<T, R>(
            IEnumerable<T> items,
            Func<T, Task<R>> processor,
            int? maxDegreeOfParallelism = null)
        {
            if (items == null) return new List<R>();
            var itemList = items.ToList();
            var results = new R[itemList.Count];
            var throttler = new SemaphoreSlim(maxDegreeOfParallelism ?? Environment.ProcessorCount);
            var tasks = new List<Task>();

            for (int i = 0; i < itemList.Count; i++)
            {
                await throttler.WaitAsync().ConfigureAwait(false);
                int index = i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        results[index] = await processor(itemList[index]).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Async parallel processing error: {ex.Message}");
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
            return results.Where(r => !(r is null)).ToList();
        }

        /// <summary>
        /// 並行處理批次數據
        /// </summary>
        public static List<R> ProcessInBatches<T, R>(
            IEnumerable<T> items,
            Func<IEnumerable<T>, IEnumerable<R>> processor,
            int batchSize = 100,
            int? maxDegreeOfParallelism = null)
        {
            if (items == null) return new List<R>();
            var batches = items.Select((item, idx) => new { item, idx })
                               .GroupBy(x => x.idx / System.Math.Max(1, batchSize))
                               .Select(g => g.Select(x => x.item).ToList())
                               .ToList();

            var results = new List<R>();
            var options = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism ?? Environment.ProcessorCount };
            var sync = new object();

            System.Threading.Tasks.Parallel.ForEach(batches, options, batch =>
            {
                try
                {
                    var batchResults = processor(batch) ?? Enumerable.Empty<R>();
                    lock (sync)
                    {
                        results.AddRange(batchResults);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Batch processing error: {ex.Message}");
                }
            });

            return results;
        }

        /// <summary>
        /// 並行處理並返回進度報告
        /// </summary>
        public static (List<R> Results, int ProcessedCount, int TotalCount) ProcessWithProgress<T, R>(
            IEnumerable<T> items,
            Func<T, R> processor,
            Action<int, int> progressCallback = null,
            int? maxDegreeOfParallelism = null)
        {
            if (items == null) return (new List<R>(), 0, 0);
            var itemList = items.ToList();
            var totalCount = itemList.Count;
            var processed = 0;
            var sync = new object();
            var results = new R[totalCount];
            var options = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism ?? Environment.ProcessorCount };

            System.Threading.Tasks.Parallel.For(0, totalCount, options, i =>
            {
                try
                {
                    var res = processor(itemList[i]);
                    results[i] = res;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Progress processing error: {ex.Message}");
                }
                finally
                {
                    lock (sync)
                    {
                        processed++;
                        progressCallback?.Invoke(processed, totalCount);
                    }
                }
            });

            return (results.Where(r => !(r is null)).ToList(), processed, totalCount);
        }

        /// <summary>
        /// 檢查系統是否支持並行處理
        /// </summary>
        public static bool IsParallelProcessingSupported()
        {
            return Environment.ProcessorCount > 1;
        }

        /// <summary>
        /// 獲取推薦的並行度
        /// </summary>
        public static int GetRecommendedParallelism(int itemCount)
        {
            if (itemCount <= 0) return 1;
            if (itemCount <= 10) return 1;
            if (itemCount <= 100) return System.Math.Min(2, Environment.ProcessorCount);
            if (itemCount <= 1000) return System.Math.Min(4, Environment.ProcessorCount);
            return Environment.ProcessorCount;
        }
    }

    /// <summary>
    /// 並行處理配置
    /// </summary>
    public class ParallelProcessingConfig
    {
        public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;
        public int BatchSize { get; set; } = 100;
        public bool EnableProgressReporting { get; set; } = true;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
        public bool EnableErrorHandling { get; set; } = true;
    }
}




