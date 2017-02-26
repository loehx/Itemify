using System;
using System.Threading.Tasks;

namespace Itemify.Logging
{
    public interface ILogWriter : IDisposable
    {
        ILogWriter Describe(string description);
        ILogWriter Describe(string description, object toDescribe);
        ILogWriter Describe(Exception err);
        ILogWriter StartStopwatch();
        ILogWriter ClearStopwatch();
        ILogWriter NewRegion(int index);
        ILogWriter NewRegion(string pascalCaseName);
        void Dispose();
    }
}