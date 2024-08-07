﻿using System;
using System.Diagnostics;

namespace SaintsField.Playa
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class PlayaHideIfAttribute: PlayaShowIfAttribute
    {
        public PlayaHideIfAttribute(EMode editorMode, params object[] orCallbacks): base(editorMode, orCallbacks)
        {
        }

        public PlayaHideIfAttribute(params object[] orCallbacks): base(orCallbacks)
        {
        }
    }
}
