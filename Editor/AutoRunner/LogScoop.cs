using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Editor.AutoRunner
{
    public class LogScoop: IDisposable
    {
        public readonly List<(string, string)> ErrorLogs = new List<(string, string)>();

        public LogScoop()
        {
            Application.logMessageReceivedThreaded += DebugLogRedirectHandle;
        }

        private void DebugLogRedirectHandle(string condition, string stacktrace, LogType type)
        {
            // ReSharper disable once MergeIntoLogicalPattern
            if(type == LogType.Error || type == LogType.Assert)
            {
                ErrorLogs.Add((condition, stacktrace));
            }
        }

        public void Dispose()
        {
            Application.logMessageReceivedThreaded -= DebugLogRedirectHandle;
        }
    }
}
