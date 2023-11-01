using UnityEngine;

namespace ExtInspector
{
    public class ResizableTextAreaAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy { get; }

        // public readonly bool FullWidth;

        public ResizableTextAreaAttribute(bool fullWidth=true)
        {
            // FullWidth = fullWidth;
            GroupBy = fullWidth ? "ResizableTextAreaAttribute" : "__LABEL_FIELD__";
        }
    }
}
