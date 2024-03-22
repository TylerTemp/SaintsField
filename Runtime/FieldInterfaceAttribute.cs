using System;
using UnityEngine;

namespace SaintsField
{
    public class FieldInterfaceAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "__LABEL_FIELD__";

        // ReSharper disable InconsistentNaming
        public readonly Type InterfaceType;
        public readonly EPick EditorPick;
        public readonly bool CustomPicker;
        // ReSharper enable InconsistentNaming

        public FieldInterfaceAttribute(Type interfaceType, EPick editorPick = EPick.Assets | EPick.Scene,
            bool customPicker = false)
        {
            InterfaceType = interfaceType;
            EditorPick = editorPick;
            CustomPicker = customPicker;

        }
    }
}
