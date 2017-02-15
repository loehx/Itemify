using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Itemify.Shared.Logging;
using Itemify.Shared.Utils;

namespace Itemify.Logging
{
    public class RegionBasedLogWriter : ILogWriter
    {
        private readonly ILogData log;
        private readonly string region;
        private readonly int level;
        private readonly int bufferSize;
        private readonly Stopwatch stopwatch;

        private List<RegionBasedLogWriter> subRegions = null;
        private List<LogEntry> buffer;
        private object syncRoot = new object();
        private Stopwatch totalStopwatch;

        public RegionBasedLogWriter(ILogData log, string region, int bufferSize = 256)
            : this(log, region, bufferSize, 0)
        {
        }

        private RegionBasedLogWriter(ILogData log, string region, int bufferSize, int level)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (region == null) throw new ArgumentNullException(nameof(region));

            this.log = log;
            this.region = formatRegionName(region);
            this.level = Math.Abs(level);
            this.bufferSize = bufferSize;
            stopwatch = new Stopwatch();
            totalStopwatch = new Stopwatch();
            this.buffer = new List<LogEntry>(bufferSize);
        }

        public ILogWriter Describe(string description)
        {
            write(description, null);
            return this;
        }

        public ILogWriter Describe(string description, object toDescribe)
        {
            var desc = toDescribe == null ? "null" : JsonUtil.Stringify(toDescribe, true);
            write(description, desc);
            return this;
        }

        public ILogWriter Describe(Exception err)
        {
            write(err.GetType().Name + ": " + err.Message, err.ToString());
            return this;
        }

        public ILogWriter NewRegion(int index)
        {
            return NewRegion("#" + index);
        }

        public ILogWriter NewRegion(string pascalCaseName)
        {
            if (pascalCaseName == null) throw new ArgumentNullException(nameof(pascalCaseName));
            Flush();

            pascalCaseName = formatRegionName(pascalCaseName);
            var subRegion = new RegionBasedLogWriter(log, region + "." + pascalCaseName, bufferSize, level + 1);

            if (stopwatch.IsRunning)
                subRegion.StartStopwatch();

            if (subRegions == null)
                subRegions = new List<RegionBasedLogWriter>();

            subRegions.Add(subRegion);

            return subRegion;
        }

        public void Flush()
        {
            subRegions?.ForEach(k => k.flush());

            if (buffer.Count == 0)
                return;

            lock (syncRoot)
            {
                flush();
            }
        }
        public async Task FlushAsync()
        {
            await Task.Yield();
            Flush();
        }

        protected void write(string message, string description)
        {
            stopwatch.Stop();
            var entry = new LogEntry(region, 
                message?.Trim(), 
                description?.Trim(), 
                stopwatch.ElapsedMilliseconds, 
                level, 
                DateTime.Now, 
                Thread.CurrentThread.ManagedThreadId);

            lock (syncRoot)
            {
                this.buffer.Add(entry);

                if (buffer.Count >= bufferSize)
                    flush();
            }

            stopwatch.Restart();
        }

        protected void flush()
        {
            var b = buffer;
            buffer = new List<LogEntry>(bufferSize);
            log.AddRange(b);
        }

        private static string formatRegionName(string regionName)
        {
            if (regionName == null) return null;

            var sb = new StringBuilder(regionName.Length);
            var except = ".#".ToCharArray();

            foreach (var c in regionName)
            {
                if (char.IsLetterOrDigit(c) || Array.IndexOf(except, c) != -1)
                    sb.Append(c);
                else
                    sb.Append('_');
            }

            return sb.ToString().Pascalize();
        }

        public ILogWriter StartStopwatch()
        {
            totalStopwatch.Restart();
            stopwatch.Restart();
            return this;
        }

        public ILogWriter ClearStopwatch()
        {
            totalStopwatch.Reset();
            stopwatch.Reset();
            return this;
        }

        public void Dispose()
        {
            if (stopwatch.IsRunning)
            {
                var runtime = TimeSpan.FromMilliseconds(totalStopwatch.ElapsedMilliseconds);
                write($"~ {runtime.ToReadableString(2, true)}", null);
                ClearStopwatch();
            }

            subRegions?.ForEach(k => k.Dispose());
            Flush();
        }
    }

}
