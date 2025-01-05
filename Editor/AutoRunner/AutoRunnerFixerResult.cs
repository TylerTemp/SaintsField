using System;

namespace SaintsField.Editor.AutoRunner
{
    // [Serializable]
    public class AutoRunnerFixerResult
    {
        public bool CanFix;
        public Action Callback;
        public string Error;
        public string ExecError;
    }
}
