using System;
using System.Collections.Generic;

namespace Itemify.Shared.Logging
{
    public interface ILogData
    {
        void AddRange(IReadOnlyList<ILogEntry> entries);
        IEnumerable<ILogEntry> Read(string regionStartsWidth, DateTime startOf, int count);
    }
}