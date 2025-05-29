using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ExpandableDrawer
{

#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.Editor.DrawerPriority(Sirenix.OdinInspector.Editor.DrawerPriorityLevel.WrapperPriority)]
#endif
    [CustomPropertyDrawer(typeof(ExpandableAttribute), true)]
    public partial class ExpandableAttributeDrawer: SaintsPropertyDrawer
    {
        private static Object GetSerObject(SerializedProperty property, MemberInfo info, object parent)
        {
            Object obj = SerializedUtils.GetSerObject(property, info, parent);
            if (obj != null)
            {
                return obj;
            }

#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
            // ReSharper disable once InvertIf
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                SerializedProperty guidProp = property.FindPropertyRelative("m_AssetGUID");
                // Debug.Log(guidProp);
                if (guidProp == null)
                {
                    return null;
                }

                string guid = guidProp.stringValue;
                if (string.IsNullOrEmpty(guid))
                {
                    return null;
                }

                string path = AssetDatabase.GUIDToAssetPath(guid);
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (string.IsNullOrEmpty(path))
                {
                    return null;
                }

                return AssetDatabase.LoadAssetAtPath<Object>(path);
            }
#endif

            return obj;
        }
    }
}
