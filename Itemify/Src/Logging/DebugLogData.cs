using System;
using System.Collections.Generic;
using System.Diagnostics;
using Itemify.Shared.Logging;
using Lustitia.Utils;

namespace Itemify.Logging
{
    internal class DebugLogData : ILogData
    {
        private int tabSize;
        private int lastThread;
        private long lastMs;

        public DebugLogData(int tabSize = 4)
        {
            this.tabSize = tabSize;
            lastMs = 0L;
            lastThread = 0;
        }

        public void AddRange(IReadOnlyList<ILogEntry> entries)
        {
            foreach (var entry in entries)
            {
                var region = $"[{entry.Region}]";
                var prefix = region + " ";
                var descPrefix = region + "    ";
                var msg = prefix + entry.Message.ReplaceLineBreaks(Environment.NewLine + prefix);

                if (entry.Milliseconds > 0)
                {
                    var tookMs = entry.Milliseconds - lastMs;
                    if (tookMs > 0)
                        msg += " +" + TimeSpan.FromMilliseconds(tookMs).ToReadableString(2, true);
                    lastMs = entry.Milliseconds;
                }

                if (entry.ThreadId != lastThread)
                {
                    msg += " #" + entry.ThreadId;

                    lastThread = entry.ThreadId;
                }

                writeLine(msg);

                if (entry.Description.IsNotEmpty())
                {
                    var description = descPrefix + entry.Description.ReplaceLineBreaks(Environment.NewLine + descPrefix);
                    writeLine(description);
                }
            }
        }

        public IEnumerable<ILogEntry> Read(string regionStartsWidth, DateTime startOf, int count)
        {
            throw new NotSupportedException();
        }

        private void writeLine(string message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            Debug.WriteLine(message);
            Console.WriteLine(message);
        }
    }
}