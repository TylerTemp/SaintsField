using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.BaseWrapTypeDrawer
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.ValuePriority)]
#endif
    [CustomPropertyDrawer(typeof(BaseWrap<>), true)]
    public partial class BaseWrapDrawer: SaintsPropertyDrawer
    {
        public static (SerializedProperty realProp, FieldInfo realInfo) GetBasicInfo(SerializedProperty property, FieldInfo info)
        {
            // string label = GetPreferredLabel(property);
            //
            SerializedProperty realProp = property.FindPropertyRelative("value") ?? SerializedUtils.FindPropertyByAutoPropertyName(property, "value");
            Debug.Assert(realProp != null, property.propertyPath);

            FieldInfo realInfo = ReflectUtils.GetElementType(info.FieldType).GetField("value", BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            return (realProp, realInfo);
        }


    }
}
