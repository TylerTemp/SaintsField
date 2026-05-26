using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field)]
    public class ScaleHandleAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Other;
        public string GroupBy => "";

        public readonly string Space;
        public readonly Vector3 PosOffset;
        public readonly string PosOffsetCallback;

        public ScaleHandleAttribute(string space = "this",
            float posXOffset = 0f, float posYOffset = 0f, float posZOffset = 0f, string posOffsetCallback = null)
        {
            Space = space;
            PosOffset = new Vector3(posXOffset, posYOffset, posZOffset);
            PosOffsetCallback = posOffsetCallback;
        }
    }
}
