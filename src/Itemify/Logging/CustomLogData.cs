using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Itemify.Shared.Utils;

namespace Itemify.Logging
{
    public class CustomLogData : ILogData
    {
        private Action<string> writeLine;
        private int tabSize;
        private readonly int maxLevel;
        private readonly List<LogEntry> entries;
        private readonly object syncRoot;
        private int lastThread;

        public CustomLogData(Action<string> writeLine, int tabSize = 4, int maxLevel = int.MaxValue)
        {
            this.writeLine = writeLine;
            this.tabSize = tabSize;
            this.maxLevel = maxLevel;
            this.entries = new List<LogEntry>();
            this.syncRoot = new object();
        }

        public void Add(LogEntry entry)
        {
            if (entry.Level > maxLevel)
                return;

            var region = $"[{entry.Region}]";
            var prefix = region + " ";
            var descPrefix = region + new string(' ', tabSize + 1);
            var msg = prefix + entry.Message;

            if (entry.Milliseconds > 0)
            {
                var tookMs = entry.Milliseconds;
                if (tookMs > 0)
                    msg += " [+" + tookMs.ToString("#,###.#" + CultureInfo.InvariantCulture) + " ms]";
            }

            if (entry.ThreadId != lastThread)
            {
                msg += " #" + entry.ThreadId;

                lastThread = entry.ThreadId;
            }

            lock (syncRoot)
            {
                this.entries.Add(entry);

                write(msg);

                if (entry.Description.IsNotEmpty())
                {
                    var description = descPrefix +
                                      entry.Description.Replace(Environment.NewLine, Environment.NewLine + descPrefix);
                    write(description);
                }
            }
        }

        public IEnumerable<LogEntry> Read(string regionStartsWidth, DateTime startOf, int count)
        {
            lock (syncRoot)
            {
                return this.entries
                    .OrderByDescending(k => k.Timestamp)
                    .Where(k => k.Region == null || k.Region.StartsWith(regionStartsWidth) && k.Timestamp > startOf)
                    .Take(count)
                    .ToArray();
            }
        }

        private void write(string message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            writeLine(message);
        }
    }
}