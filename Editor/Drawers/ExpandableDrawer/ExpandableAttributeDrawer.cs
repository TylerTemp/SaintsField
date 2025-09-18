using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using SaintsField.Utils;
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
        private static SerializedObject GetSerObject(SerializedProperty property, MemberInfo info, object parent)
        {
            List<Object> targets = new List<Object>();
            string propPath = property.propertyPath;
            string addressablePropGuidPath = null;

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

                addressablePropGuidPath = guidProp.propertyPath;

                // string guid = guidProp.stringValue;
                // if (string.IsNullOrEmpty(guid))
                // {
                //     return null;
                // }
                //
                // string path = AssetDatabase.GUIDToAssetPath(guid);
                // // ReSharper disable once ConvertIfStatementToReturnStatement
                // if (string.IsNullOrEmpty(path))
                // {
                //     return null;
                // }
                //
                // return AssetDatabase.LoadAssetAtPath<Object>(path);
            }
#endif

            foreach (Object serTargetObject in property.serializedObject.targetObjects)
            {
                // ReSharper disable once ConvertToUsingDeclaration
                using(SerializedObject serObj = new SerializedObject(serTargetObject))
                {
#if SAINTSFIELD_ADDRESSABLE && !SAINTSFIELD_ADDRESSABLE_DISABLE
                    // ReSharper disable once UseNegatedPatternInIsExpression
                    if (!(addressablePropGuidPath is null))
                    {
                        // find addressable target
                        SerializedProperty guidProp = serObj.FindProperty(addressablePropGuidPath);
                        if (guidProp == null)
                        {
                            continue;
                        }
                        string guid = guidProp.stringValue;
                        if (string.IsNullOrEmpty(guid))
                        {
                            continue;
                        }
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (string.IsNullOrEmpty(path))
                        {
                            continue;
                        }
                        Object addressableObj = AssetDatabase.LoadAssetAtPath<Object>(path);
                        if (addressableObj == null)
                        {
                            continue;
                        }

                        targets.Add(addressableObj);
                        continue;
                    }
#endif
                    SerializedProperty serProp = serObj.FindProperty(propPath);
                    Object obj = SerializedUtils.GetSerObject(serProp, info, parent);
                    if(!RuntimeUtil.IsNull(obj))
                    {
                        targets.Add(obj);
                    }
                }
            }

            if(targets.Count == 0)
            {
                return null;
            }

            return new SerializedObject(targets.ToArray());
        }

        private static bool EqualSerObject(SerializedObject serObj1, SerializedObject serObj2)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if(serObj1 == null && serObj2 == null)
            {
                return true;
            }

            if (serObj1 == null)
            {
                return false;
            }

            if (serObj2 == null)
            {
                return false;
            }

            HashSet<int> instanceIds = new HashSet<int>();
            foreach (Object target in serObj1.targetObjects)
            {
                if(target)
                {
                    instanceIds.Add(target.GetInstanceID());
                }
            }

            HashSet<int> instanceIds2 = new HashSet<int>();
            foreach (Object target in serObj2.targetObjects)
            {
                // ReSharper disable once InvertIf
                if(target)
                {
                    int id = target.GetInstanceID();
                    if (!instanceIds.Contains(id))
                    {
                        return false;
                    }

                    instanceIds2.Add(id);
                }
            }

            instanceIds.ExceptWith(instanceIds2);
            return instanceIds.Count == 0;
        }
    }
}
