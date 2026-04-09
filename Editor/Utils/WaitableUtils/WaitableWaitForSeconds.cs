using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Utils.WaitableUtils
{
    public class WaitableWaitForSeconds: IWaitable
    {
        private static FieldInfo _fieldMSeconds;
        private readonly double _endTime;
        private readonly double _startTime;

        public WaitableWaitForSeconds(WaitForSeconds waitForSeconds)
        {
            _fieldMSeconds ??= typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(_fieldMSeconds != null);
            float mSeconds = (float)_fieldMSeconds.GetValue(waitForSeconds);
            _startTime = EditorApplication.timeSinceStartup;
            _endTime = _startTime + mSeconds;
        }

        public WaitableWaitForSeconds(WaitForSecondsRealtime waitForSeconds)
        {
            float mSeconds = waitForSeconds.waitTime;
            _startTime = EditorApplication.timeSinceStartup;
            _endTime = _startTime + mSeconds;
        }

        public bool Done { get; private set; }
        public float Progress { get; private set; } = 1f;

        public void Update()
        {
            double curTime = EditorApplication.timeSinceStartup;
            Done = EditorApplication.timeSinceStartup > _endTime;
            Progress = (float)(1d - InverseLerp(_startTime, _endTime, curTime));
        }

        private static double InverseLerp(double a, double b, double value)
        {
            if (b - a < Mathf.Epsilon)
            {
                return 1;
            }

            return (value - a) / (b - a);
        }
    }
}
