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

        public override string ToString()
        {
            return $"{nameof(CanFix)}: {CanFix}, {nameof(Error)}: {Error}, {nameof(ExecError)}: {ExecError}";
        }
    }
}
