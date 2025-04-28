using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.SaintsRowDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.SaintsDictionary.DictionaryWrap
{
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.SuperPriority)]
#endif
    [CustomPropertyDrawer(typeof(SaintsDictionaryBase<,>.Wrap<>), true)]
    public partial class SaintsDictionaryWrapDrawer: SaintsPropertyDrawer
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
