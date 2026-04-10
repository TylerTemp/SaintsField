using UnityEditor;

namespace SaintsField.Editor.Drawers.SaintsDecimalType
{
    public class SaintsDecimalField: SaintsDecimalFieldAbs
    {
        public SaintsDecimalField(string label) : base(label)
        {
        }

        protected override SerializedProperty GetFlagsProp(SerializedProperty property) => property.FindPropertyRelative(nameof(SaintsDecimal.flags));

        protected override SerializedProperty GetHiProp(SerializedProperty property) => property.FindPropertyRelative(nameof(SaintsDecimal.hi));

        protected override SerializedProperty GetLoProp(SerializedProperty property) => property.FindPropertyRelative(nameof(SaintsDecimal.lo));

        protected override SerializedProperty GetMidProp(SerializedProperty property) => property.FindPropertyRelative(nameof(SaintsDecimal.mid));
    }
}
