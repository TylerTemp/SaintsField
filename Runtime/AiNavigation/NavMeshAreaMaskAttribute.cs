﻿using System.Diagnostics;
using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField.AiNavigation
{
#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
    [Conditional("UNITY_EDITOR")]
    public class NavMeshAreaMaskAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy { get; }

        public NavMeshAreaMaskAttribute(string groupBy = "")
        {
            GroupBy = groupBy;
        }
    }
#endif
}
