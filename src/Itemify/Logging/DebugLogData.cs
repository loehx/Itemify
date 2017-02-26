using System;
using System.Diagnostics;

namespace Itemify.Logging
{
    [Obsolete("Please use CustomLogData instead.", true)]
    public class DebugLogData : CustomLogData
    {
        public DebugLogData(int tabSize = 4)
            : base(writeLine, tabSize)
        {
        }

        private static void writeLine(string message)
        {
            Debug.WriteLine(message);
            Console.WriteLine(message);
        }
    }
}