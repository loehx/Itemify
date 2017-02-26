using System;
using System.Collections.Generic;

namespace Itemify.Logging
{
    internal class SynchronizedLogDataWrapper : ILogData
    {
        private readonly ILogData _data;
        private object syncRoot = new object();

        public SynchronizedLogDataWrapper(ILogData data)
        {
            _data = data;
        }

        public IEnumerable<LogEntry> Read(string regionStartsWidth, DateTime startOf, int count)
        {
            lock (syncRoot)
            {
                foreach (var logEntry in _data.Read(regionStartsWidth, startOf, count))
                    yield return logEntry;
            }
        }

        public void Add(LogEntry entry)
        {
            lock (syncRoot)
            {
                _data.Add(entry);
            }
        }
    }
}
