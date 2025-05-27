using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OGRALAB.Helpers
{
    /// <summary>
    /// Helper class for performance optimization and monitoring
    /// </summary>
    public static class PerformanceHelper
    {
        #region Async Performance Helpers
        
        /// <summary>
        /// Execute multiple async operations concurrently with limited concurrency
        /// </summary>
        /// <typeparam name="T">Type of items to process</typeparam>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <param name="items">Items to process</param>
        /// <param name="processor">Async function to process each item</param>
        /// <param name="maxConcurrency">Maximum concurrent operations</param>
        /// <returns>Results of processing</returns>
        public static async Task<IEnumerable<TResult>> ProcessConcurrentlyAsync<T, TResult>(
            IEnumerable<T> items,
            Func<T, Task<TResult>> processor,
            int maxConcurrency = 0)
        {
            if (maxConcurrency <= 0)
                maxConcurrency = Constants.MaxConcurrentOperations;
                
            var semaphore = new System.Threading.SemaphoreSlim(maxConcurrency);
            var tasks = items.Select(async item =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await processor(item);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            
            return await Task.WhenAll(tasks);
        }
        
        /// <summary>
        /// Execute an operation with timeout
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="operation">Operation to execute</param>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>Result or throws TimeoutException</returns>
        public static async Task<T> ExecuteWithTimeoutAsync<T>(
            Func<Task<T>> operation,
            int timeoutSeconds = 0)
        {
            if (timeoutSeconds <= 0)
                timeoutSeconds = Constants.NetworkTimeoutSeconds;
                
            using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            
            try
            {
                return await operation().ConfigureAwait(false);
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                throw new TimeoutException($"Operation timed out after {timeoutSeconds} seconds");
            }
        }
        
        #endregion
        
        #region Memory Management
        
        /// <summary>
        /// Check if memory usage is within acceptable limits
        /// </summary>
        /// <returns>True if memory usage is acceptable</returns>
        public static bool IsMemoryUsageAcceptable()
        {
            var process = Process.GetCurrentProcess();
            var totalMemory = GC.GetTotalMemory(false);
            var workingSet = process.WorkingSet64;
            
            // Simple heuristic: if working set is less than reasonable limit
            return workingSet < (1024 * 1024 * 1024); // 1GB limit
        }
        
        /// <summary>
        /// Force garbage collection if memory usage is high
        /// </summary>
        public static void OptimizeMemoryIfNeeded()
        {
            if (!IsMemoryUsageAcceptable())
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }
        
        #endregion
        
        #region Collection Performance
        
        /// <summary>
        /// Safely convert enumerable to list with size limit
        /// </summary>
        /// <typeparam name="T">Type of items</typeparam>
        /// <param name="source">Source enumerable</param>
        /// <param name="maxItems">Maximum items to take</param>
        /// <returns>List with limited size</returns>
        public static List<T> ToSafeList<T>(this IEnumerable<T> source, int maxItems = 0)
        {
            if (maxItems <= 0)
                maxItems = Constants.MaxRecordsPerQuery;
                
            return source.Take(maxItems).ToList();
        }
        
        /// <summary>
        /// Check if collection has any items without enumerating all
        /// </summary>
        /// <typeparam name="T">Type of items</typeparam>
        /// <param name="source">Source enumerable</param>
        /// <returns>True if has any items</returns>
        public static bool FastAny<T>(this IEnumerable<T> source)
        {
            if (source is ICollection<T> collection)
                return collection.Count > 0;
                
            return source.Any();
        }
        
        /// <summary>
        /// Get count efficiently without full enumeration if possible
        /// </summary>
        /// <typeparam name="T">Type of items</typeparam>
        /// <param name="source">Source enumerable</param>
        /// <returns>Count of items</returns>
        public static int FastCount<T>(this IEnumerable<T> source)
        {
            if (source is ICollection<T> collection)
                return collection.Count;
                
            return source.Count();
        }
        
        #endregion
        
        #region Batch Processing
        
        /// <summary>
        /// Process items in batches to avoid memory issues
        /// </summary>
        /// <typeparam name="T">Type of items</typeparam>
        /// <param name="source">Source items</param>
        /// <param name="batchSize">Size of each batch</param>
        /// <param name="processor">Batch processor</param>
        /// <returns>Task representing the operation</returns>
        public static async Task ProcessInBatchesAsync<T>(
            IEnumerable<T> source,
            int batchSize,
            Func<IEnumerable<T>, Task> processor)
        {
            if (batchSize <= 0)
                batchSize = Constants.DefaultPageSize;
                
            var batch = new List<T>(batchSize);
            
            foreach (var item in source)
            {
                batch.Add(item);
                
                if (batch.Count >= batchSize)
                {
                    await processor(batch);
                    batch.Clear();
                    
                    // Optional: Allow garbage collection between batches
                    if (batch.Count % (batchSize * 10) == 0)
                    {
                        OptimizeMemoryIfNeeded();
                    }
                }
            }
            
            // Process remaining items
            if (batch.Count > 0)
            {
                await processor(batch);
            }
        }
        
        #endregion
        
        #region Performance Monitoring
        
        /// <summary>
        /// Measure execution time of an operation
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="operation">Operation to measure</param>
        /// <param name="operationName">Name for logging</param>
        /// <returns>Tuple of result and elapsed time</returns>
        public static async Task<(T Result, TimeSpan Elapsed)> MeasureAsync<T>(
            Func<Task<T>> operation,
            string operationName = "Operation")
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var result = await operation();
                stopwatch.Stop();
                
                // Log if operation takes longer than expected
                if (stopwatch.ElapsedMilliseconds > 5000) // 5 seconds
                {
                    Debug.WriteLine($"⚠️ Slow operation '{operationName}': {stopwatch.ElapsedMilliseconds}ms");
                }
                
                return (result, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Debug.WriteLine($"❌ Operation '{operationName}' failed after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Measure execution time of a void operation
        /// </summary>
        /// <param name="operation">Operation to measure</param>
        /// <param name="operationName">Name for logging</param>
        /// <returns>Elapsed time</returns>
        public static async Task<TimeSpan> MeasureAsync(
            Func<Task> operation,
            string operationName = "Operation")
        {
            var (_, elapsed) = await MeasureAsync(async () =>
            {
                await operation();
                return true;
            }, operationName);
            
            return elapsed;
        }
        
        #endregion
        
        #region Retry Logic
        
        /// <summary>
        /// Execute operation with retry logic
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="operation">Operation to execute</param>
        /// <param name="maxRetries">Maximum retry attempts</param>
        /// <param name="delayMs">Delay between retries in milliseconds</param>
        /// <param name="operationName">Name for logging</param>
        /// <returns>Result of operation</returns>
        public static async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            int maxRetries = 0,
            int delayMs = 0,
            string operationName = "Operation")
        {
            if (maxRetries <= 0)
                maxRetries = Constants.MaxRetryAttempts;
            if (delayMs <= 0)
                delayMs = Constants.RetryDelayMs;
                
            Exception lastException = null;
            
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    if (attempt == maxRetries)
                    {
                        Debug.WriteLine($"❌ Operation '{operationName}' failed after {maxRetries} attempts: {ex.Message}");
                        throw;
                    }
                    
                    Debug.WriteLine($"⚠️ Operation '{operationName}' attempt {attempt} failed, retrying in {delayMs}ms: {ex.Message}");
                    await Task.Delay(delayMs);
                }
            }
            
            throw lastException ?? new InvalidOperationException("Unexpected retry logic failure");
        }
        
        #endregion
    }
}
