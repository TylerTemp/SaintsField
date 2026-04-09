using System;
using System.Reflection;
using UnityEngine;

namespace SaintsField.Editor.Utils.WaitableUtils
{
    public class WaitableWaitForCallback: IWaitable
    {
        private readonly Func<bool> _mPredicate;
        private static FieldInfo _fieldWaitUntilMPredicate;
        private static FieldInfo _fieldWaitUntilMTimeoutCallback;

        public WaitableWaitForCallback(WaitUntil waitUntil)
        {
            _fieldWaitUntilMPredicate ??= typeof(WaitUntil).GetField("m_Predicate", BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Assert(_fieldWaitUntilMPredicate != null);
            _mPredicate = (Func<bool>)_fieldWaitUntilMPredicate.GetValue(waitUntil);

            _fieldWaitUntilMTimeoutCallback ??= typeof(WaitUntil).GetField("m_TimeoutCallback", BindingFlags.Instance | BindingFlags.NonPublic);

            // ReSharper disable once InvertIf
            if(_fieldWaitUntilMTimeoutCallback != null)
            {
                object mTimeoutCallback = _fieldWaitUntilMTimeoutCallback.GetValue(waitUntil);
                if (mTimeoutCallback != null)
                {
                    throw new Exception("Timeout is not supported in editor button");
                }
            }
        }

        private static FieldInfo _fieldWaitWhileMPredicate;

        public WaitableWaitForCallback(WaitWhile waitWhile)
        {
            _fieldWaitWhileMPredicate ??= typeof(WaitWhile).GetField("m_Predicate", BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Assert(_fieldWaitWhileMPredicate != null);
            Func<bool> predicate = (Func<bool>)_fieldWaitWhileMPredicate.GetValue(waitWhile);
            _mPredicate = () => !predicate();
        }

        public bool Done { get; private set; }
        public float Progress => -1f;

        public void Update()
        {
            if (!Done)
            {
                Done = _mPredicate();
            }
        }
    }
}
