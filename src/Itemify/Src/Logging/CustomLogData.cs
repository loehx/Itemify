using System;
using System.Collections.Generic;
using System.Globalization;
using Itemify.Shared.Logging;
using Lustitia.Utils;

namespace Itemify.Logging
{
    public class CustomLogData : ILogData
    {
        private Action<string> writeLine;
        private int tabSize;
        private readonly int maxLevel;

        public CustomLogData(Action<string> writeLine, int tabSize = 4, int maxLevel = int.MaxValue)
        {
            this.writeLine = writeLine;
            this.tabSize = tabSize;
            this.maxLevel = maxLevel;
        }

        public void AddRange(IReadOnlyList<ILogEntry> entries)
        { 
            var lastThread = 0;

            foreach (var entry in entries)
            {
                if (entry.Level > maxLevel)
                    continue;

                var region = $"[{entry.Region}]";
                var prefix = region + " ";
                var descPrefix = region + new string(' ', tabSize + 1);
                var msg = prefix + entry.Message
                        .Replace("\r", "")
                        .Replace("\n", Environment.NewLine + descPrefix); ;

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

                write(msg);

                if (entry.Description.IsNotEmpty())
                {
                    var description = descPrefix + entry.Description
                        .Replace("\r", "")
                        .Replace("\n", Environment.NewLine + descPrefix);
                    write(description);
                }
            }
        }

        public IEnumerable<ILogEntry> Read(string regionStartsWidth, DateTime startOf, int count)
        {
            throw new NotSupportedException();
        }

        private void write(string message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            writeLine(message);
        }
    }
}