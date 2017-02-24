using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Humanizer;
using Itemify.Shared.Utils;

namespace Itemify.Logging
{


    public class RegionBasedLogWriter : ILogWriter
    {
        private readonly ILogData log;
        private readonly string region;
        private readonly int level;
        private readonly Stopwatch stopwatch;

        private ConcurrentBag<RegionBasedLogWriter> subRegions = null;
        private Stopwatch totalStopwatch;

        public RegionBasedLogWriter(ILogData log, string region)
            : this(new SynchronizedLogDataWrapper(log), region, 0)
        {
        }

        private RegionBasedLogWriter(ILogData log, string region, int level, object sharedSyncRoot = null)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (region == null) throw new ArgumentNullException(nameof(region));

            this.log = log;
            this.region = formatRegionName(region);
            this.level = Math.Abs(level);
            stopwatch = new Stopwatch();
            totalStopwatch = new Stopwatch();
            subRegions = new ConcurrentBag<RegionBasedLogWriter>();
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

            pascalCaseName = formatRegionName(pascalCaseName);
            var subRegion = new RegionBasedLogWriter(log, region + "." + pascalCaseName, level + 1);

            if (stopwatch.IsRunning)
                subRegion.StartStopwatch();

            subRegions.Add(subRegion);

            return subRegion;
        }


        protected void write(string message, string description)
        {
            var entry = new LogEntry(region,
                message?.Trim(),
                description?.Trim(),
                stopwatch.ElapsedMilliseconds,
                level,
                DateTime.Now,
                Thread.CurrentThread.ManagedThreadId);

                log.Add(entry);
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
        }
    }

}
