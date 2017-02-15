using System;
using Itemify.Shared.Logging;

namespace Itemify.Logging
{
    public class LogEntry : ILogEntry
    {
        public LogEntry(string region, string message, string description, long milliseconds, int level, DateTime timestamp, int threadId)
        {
            Region = region;
            Message = message;
            Description = description;
            Milliseconds = milliseconds;
            Level = level;
            Timestamp = timestamp;
            ThreadId = threadId;
        }

        public string Region { get; }
        public string Message { get; }
        public string Description { get; }
        public long Milliseconds { get; }
        public int Level { get; }
        public DateTime Timestamp { get; }
        public int ThreadId { get; }

        // TODO: Implement GetHashCode, Equals, ToString
    }
}