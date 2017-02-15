using System;

namespace Itemify.Shared.Logging
{
    public interface ILogEntry
    {
        string Description { get; }
        int Level { get; }
        string Message { get; }
        long Milliseconds { get; }
        string Region { get; }
        int ThreadId { get; }
        DateTime Timestamp { get; }
    }
}