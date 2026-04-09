using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.WaitableUtils
{
    public class WaitableWaitForSeconds: IWaitable
    {
        private static FieldInfo _fieldMSeconds;
        private readonly double _endTime;

        public WaitableWaitForSeconds(WaitForSeconds waitForSeconds)
        {
            _fieldMSeconds ??= typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(_fieldMSeconds != null);
            float mSeconds = (float)_fieldMSeconds.GetValue(waitForSeconds);
            double curTime = EditorApplication.timeSinceStartup;
            _endTime = curTime + mSeconds;
        }

        public WaitableWaitForSeconds(WaitForSecondsRealtime waitForSeconds)
        {
            float mSeconds = waitForSeconds.waitTime;
            double curTime = EditorApplication.timeSinceStartup;
            _endTime = curTime + mSeconds;
        }

        public bool Done { get; private set; }
        public void Update()
        {
            Done = EditorApplication.timeSinceStartup > _endTime;
        }
    }
}
