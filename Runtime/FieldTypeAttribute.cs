using System;
using System.Diagnostics;
using UnityEngine;

namespace SaintsField
{
    [Conditional("UNITY_EDITOR")]
    public class FieldTypeAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        // ReSharper disable InconsistentNaming
        public readonly Type CompType;
        public readonly EPick EditorPick;
        public readonly bool CustomPicker;
        // ReSharper enable InconsistentNaming

        public FieldTypeAttribute(Type compType, EPick editorPick = EPick.Assets | EPick.Scene, bool customPicker = true)
        {
            CompType = compType;
            EditorPick = editorPick == 0
                ? EPick.Assets | EPick.Scene
                : editorPick;
            CustomPicker = customPicker;
        }

        public FieldTypeAttribute(Type compType, bool customPicker): this(compType, EPick.Assets | EPick.Scene, customPicker)
        {
        }

        public FieldTypeAttribute(EPick editorPick = EPick.Assets | EPick.Scene, bool customPicker = true): this(null, editorPick, customPicker)
        {
        }
    }
}
