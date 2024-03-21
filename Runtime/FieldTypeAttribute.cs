using System;
using UnityEngine;

namespace SaintsField
{
    public class FieldTypeAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        // ReSharper disable InconsistentNaming
        public readonly Type CompType;
        public readonly EPick EditorPick;
        public readonly bool CustomPicker;
        // ReSharper enable InconsistentNaming

        public FieldTypeAttribute(Type compType, EPick editorPick = EPick.Assets | EPick.Scene,
            bool customPicker = false)
        {
            CompType = compType;
            EditorPick = editorPick;
            CustomPicker = customPicker;

        }
    }
}
