using System;
using System.Diagnostics;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class HideIfAttribute: ShowIfAttribute
    {
        public HideIfAttribute(EMode editorMode, params string[] orCallbacks) : base(editorMode, orCallbacks)
        {
        }

        public HideIfAttribute(params string[] orCallbacks) : base(orCallbacks)
        {
        }

        #region Enum 1-4

        public HideIfAttribute(EMode editorMode, string callback1, object enumTarget1): base(editorMode, callback1, enumTarget1){}

        public HideIfAttribute(string callback1, object enumTarget1): base(callback1, enumTarget1){}

        public HideIfAttribute(EMode editorMode, string callback1, object enumTarget1, string callback2, object enumTarget2): base(editorMode, callback1, enumTarget1, callback2, enumTarget2){}

        public HideIfAttribute(string callback1, object enumTarget1, string callback2, object enumTarget2): base(callback1, enumTarget1, callback2, enumTarget2){}

        public HideIfAttribute(EMode editorMode, string callback1, object enumTarget1, string callback2, object enumTarget2, string callback3, object enumTarget3): base(editorMode, callback1, enumTarget1, callback2, enumTarget2, callback3, enumTarget3){}

        public HideIfAttribute(string callback1, object enumTarget1, string callback2, object enumTarget2, string callback3, object enumTarget3): base(callback1, enumTarget1, callback2, enumTarget2, callback3, enumTarget3){}

        public HideIfAttribute(EMode editorMode, string callback1, object enumTarget1, string callback2, object enumTarget2, string callback3, object enumTarget3, string callback4, object enumTarget4): base(editorMode, callback1, enumTarget1, callback2, enumTarget2, callback3, enumTarget3, callback4, enumTarget4){}

        public HideIfAttribute(string callback1, object enumTarget1, string callback2, object enumTarget2, string callback3, object enumTarget3, string callback4, object enumTarget4): base(callback1, enumTarget1, callback2, enumTarget2, callback3, enumTarget3, callback4, enumTarget4){}
        #endregion

        #region string+enum 2-4

        // 1+1
        public HideIfAttribute(EMode editorMode, string normalCallback, string enumCallback1, object enumTarget1): base(editorMode, normalCallback, enumCallback1, enumTarget1){}
        public HideIfAttribute(string normalCallback, string enumCallback1, object enumTarget1): base(normalCallback, enumCallback1, enumTarget1){}

        // 1+2
        public HideIfAttribute(EMode editorMode, string normalCallback, string enumCallback1, object enumTarget1, string enumCallback2, object enumTarget2): base(editorMode, normalCallback, enumCallback1, enumTarget1, enumCallback2, enumTarget2){}

        public HideIfAttribute(string normalCallback, string enumCallback1, object enumTarget1, string enumCallback2, object enumTarget2): base(normalCallback, enumCallback1, enumTarget1, enumCallback2, enumTarget2){}

        // 1+3
        public HideIfAttribute(EMode editorMode, string normalCallback, string enumCallback1, object enumTarget1, string enumCallback2, object enumTarget2, string enumCallback3, object enumTarget3): base(editorMode, normalCallback, enumCallback1, enumTarget1, enumCallback2, enumTarget2, enumCallback3, enumTarget3){}
        public HideIfAttribute(string normalCallback, string enumCallback1, object enumTarget1, string enumCallback2, object enumTarget2, string enumCallback3, object enumTarget3): base(normalCallback, enumCallback1, enumTarget1, enumCallback2, enumTarget2, enumCallback3, enumTarget3){}

        // 2+1
        public HideIfAttribute(EMode editorMode, string normalCallback1, string normalCallback2, string enumCallback1, object enumTarget1): base(editorMode, normalCallback1, normalCallback2, enumCallback1, enumTarget1){}
        public HideIfAttribute(string normalCallback1, string normalCallback2, string enumCallback1, object enumTarget1): base(normalCallback1, normalCallback2, enumCallback1, enumTarget1){}

        // 2+2
        public HideIfAttribute(EMode editorMode, string normalCallback1, string normalCallback2, string enumCallback1, object enumTarget1, string enumCallback2, object enumTarget2): base(editorMode, normalCallback1, normalCallback2, enumCallback1, enumTarget1, enumCallback2, enumTarget2) {}
        public HideIfAttribute(string normalCallback1, string normalCallback2, string enumCallback1, object enumTarget1, string enumCallback2, object enumTarget2): base(normalCallback1, normalCallback2, enumCallback1, enumTarget1, enumCallback2, enumTarget2){}
        // 3+1
        public HideIfAttribute(EMode editorMode, string normalCallback1, string normalCallback2, string normalCallback3, string enumCallback1, object enumTarget1): base(editorMode, normalCallback1, normalCallback2, normalCallback3, enumCallback1, enumTarget1){}

        public HideIfAttribute(string normalCallback1, string normalCallback2, string normalCallback3, string enumCallback1, object enumTarget1): base(normalCallback1, normalCallback2, normalCallback3, enumCallback1, enumTarget1){}

        #endregion
    }
}
