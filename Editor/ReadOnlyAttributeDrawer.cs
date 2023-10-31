using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExtInspector.Editor.Standalone;
using ExtInspector.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";

        protected override (bool isActive, Rect position) DrawPreLabel(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            EditorGUI.BeginDisabledGroup(IsDisabled(property, (ReadOnlyAttribute)saintsAttribute));
            return (true, position);
        }

        protected override bool DrawPostField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            EditorGUI.EndDisabledGroup();
            return true;
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return _error != "";
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            if (_error == "")
            {
                return position;
            }

            (Rect errorRect, Rect leftRect) = RectUtils.SplitHeightRect(position, HelpBox.GetHeight(_error, position.width));
            HelpBox.Draw(errorRect, _error, MessageType.Error);
            return leftRect;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute)
        {
            // Debug.Log("check extra height!");
            if (_error == "")
            {
                return 0;
            }

            // Debug.Log(HelpBox.GetHeight(_error));
            return HelpBox.GetHeight(_error, width);
        }

        private bool IsDisabled(SerializedProperty property, ReadOnlyAttribute targetAttribute)
        {
            string by = targetAttribute.ReadOnlyBy;
            if(by is null)
            {
                return targetAttribute.ReadOnlyDirectValue;
            }

            UnityEngine.Object target = property.serializedObject.targetObject;
            List<Type> types = ReflectUil.GetSelfAndBaseTypes(property.serializedObject.targetObject);
            foreach (Type systemType in types)
            {
                foreach (FieldInfo objFiledInfo in systemType
                             .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                        BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    // Debug.LogError(objFiledInfo.Name);
                    // ReSharper disable once InvertIf
                    if (objFiledInfo.Name == by || objFiledInfo.Name == $"<{by}>k__BackingField")
                    {
                        _error = "";

                        object fieldValue = null;
                        bool result;
                        try
                        {
                            fieldValue = objFiledInfo.GetValue(target);
                            // result = (objFiledInfo.GetValue(target) == null) || (bool) fieldValue;
                            // if (fieldValue)
                            // {
                            //     result = true;
                            // }
                            result = Convert.ToBoolean(fieldValue);
                        }
                        catch (InvalidCastException)
                        {
                            bool equalNull = fieldValue == null;
                            if (equalNull)
                            {
                                result = false;
                            }
                            else
                            {
                                try
                                {
                                    result = (UnityEngine.Object)fieldValue == null;
                                }
                                catch (InvalidCastException)
                                {
                                    result = true;
                                }
                            }
                        }
                        catch (NullReferenceException)
                        {
                            result = false;
                        }

                        // Debug.Log($"{by} = {result} / {objFiledInfo.GetValue(target)}");
                        return result;
                    }
                }

                foreach (MethodInfo objMethodInfo in systemType
                             .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                         BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    // ReSharper disable once InvertIf
                    if (objMethodInfo.Name == by)
                    {
                        _error = "";

                        ParameterInfo[] methodParams = objMethodInfo.GetParameters();
                        Debug.Assert(methodParams.All(p => p.IsOptional));
                        Debug.Assert(objMethodInfo.ReturnType == typeof(bool));
                        return (bool)objMethodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
                    }
                }
            }

            _error = $"No field or method named `{by}` found on `{target}`";
            Debug.LogError(_error);
            return false;
        }
    }
}
