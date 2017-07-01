using System;
using System.Collections.Generic;
using System.Text;

namespace Itemify.Logging
{
    public class EmptyLogWriter : ILogWriter
    {
        public ILogWriter Describe(string description)
        {
            return this;
        }

        public ILogWriter Describe(string description, object toDescribe)
        {
            return this;
        }

        public ILogWriter Describe(Exception err)
        {
            return this;
        }

        public ILogWriter StartStopwatch()
        {
            return this;
        }

        public ILogWriter ClearStopwatch()
        {
            return this;
        }

        public ILogWriter NewRegion(int index)
        {
            return this;
        }

        public ILogWriter NewRegion(string pascalCaseName)
        {
            return this;
        }

        void ILogWriter.Dispose()
        {
        }

        void IDisposable.Dispose()
        {
        }
    }
}
