using System;
using System.Collections.Generic;
using Itemify.Logging;

namespace Itemify.Logging
{
    public interface ILogData
    {
        IEnumerable<LogEntry> Read(string regionStartsWidth, DateTime startOf, int count);
        void Add(LogEntry entry);
    }
}