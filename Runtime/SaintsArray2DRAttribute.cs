using SaintsField.Interfaces;
using UnityEngine;

namespace SaintsField
{
    // ReSharper disable once InconsistentNaming
    public class SaintsArray2DRAttribute: PropertyAttribute, ISaintsAttribute
    {
        public SaintsAttributeType AttributeType => SaintsAttributeType.Field;
        public string GroupBy => "";

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public bool Transpose;

        public SaintsArray2DRAttribute(bool transpose = false)
        {
            Transpose = transpose;
        }
    }
}
