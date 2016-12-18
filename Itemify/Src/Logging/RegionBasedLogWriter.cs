using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Itemify.Shared.Logging;
using Itemify.Shared.Utils;

namespace Itemify.Logging
{
    internal class RegionBasedLogWriter : ILogWriter
    {
        private readonly ILogData log;
        private readonly string region;
        private readonly int level;
        private readonly int bufferSize;
        private readonly Stopwatch stopwatch;
        
        private List<LogEntry> buffer;
        private object syncRoot = new object();

        public RegionBasedLogWriter(ILogData log, string region, int bufferSize = 256)
            : this(log, region, bufferSize, 0)
        {
        }

        private RegionBasedLogWriter(ILogData log, string region, int bufferSize, int level)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (region == null) throw new ArgumentNullException(nameof(region));

            this.log = log;
            this.region = region.Pascalize();
            this.level = Math.Abs(level);
            this.bufferSize = bufferSize;
            stopwatch = new Stopwatch();
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

        public ILogWriter NewRegion(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            Flush();

            var subRegion = new RegionBasedLogWriter(log, region + "." + name.Pascalize(), bufferSize, level + 1);

            if (stopwatch.IsRunning)
                subRegion.StartStopwatch();

            return subRegion;
        }

        public void Flush()
        {
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
            var entry = new LogEntry(region, message?.Trim(), description?.Trim(), stopwatch.ElapsedMilliseconds, level, DateTime.Now, Thread.CurrentThread.ManagedThreadId);

            lock (syncRoot)
            {
                this.buffer.Add(entry);

                if (buffer.Count >= bufferSize)
                    flush();
            }
        }

        protected void flush()
        {
            var b = buffer;
            buffer = new List<LogEntry>(bufferSize);
            log.AddRange(b);
        }

        public ILogWriter StartStopwatch()
        {
            stopwatch.Restart();
            return this;
        }

        public ILogWriter ClearStopwatch()
        {
            stopwatch.Reset();
            return this;
        }

        public void Dispose()
        {
            Flush();
        }
    }

}
