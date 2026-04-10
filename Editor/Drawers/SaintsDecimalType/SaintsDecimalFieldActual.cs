using SaintsField.SaintsSerialization;
using UnityEditor;

namespace SaintsField.Editor.Drawers.SaintsDecimalType
{
    public class SaintsDecimalFieldActual: SaintsDecimalFieldAbs
    {

        public SaintsDecimalFieldActual(string label) : base(label)
        {
        }

        // lo = bits[0];
        // mid = bits[1];
        // hi = bits[2];
        // flags = bits[3];
        protected override SerializedProperty GetFlagsProp(SerializedProperty property) => property.FindPropertyRelative(nameof(SaintsSerializedProperty.intValues)).GetArrayElementAtIndex(3);

        protected override SerializedProperty GetHiProp(SerializedProperty property) => property.FindPropertyRelative(nameof(SaintsSerializedProperty.intValues)).GetArrayElementAtIndex(2);

        protected override SerializedProperty GetLoProp(SerializedProperty property) => property.FindPropertyRelative(nameof(SaintsSerializedProperty.intValues)).GetArrayElementAtIndex(0);

        protected override SerializedProperty GetMidProp(SerializedProperty property) => property.FindPropertyRelative(nameof(SaintsSerializedProperty.intValues)).GetArrayElementAtIndex(1);
    }
}
