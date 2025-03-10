// #if UNITY_2022_2_OR_NEWER || SAINTSFIELD_UI_TOOLKIT_DISABLE
using System;
using System.Diagnostics;
using SaintsField.Interfaces;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TableAttribute: PropertyAttribute, ISaintsAttribute, IPlayaAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";
        // public readonly bool DefaultExpanded;
        public readonly bool HideAddButton;
        public readonly bool HideRemoveButton;

        public TableAttribute(bool hideAddButton=false, bool hideRemoveButton=false)
        {
            // DefaultExpanded = defaultExpanded;
            HideAddButton = hideAddButton;
            HideRemoveButton = hideRemoveButton;
        }
    }
}
// #endif
