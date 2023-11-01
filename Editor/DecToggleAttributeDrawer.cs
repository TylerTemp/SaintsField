using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor
{
    public abstract class DecToggleAttributeDrawer: SaintsPropertyDrawer
    {
        protected string _error = "";
        // private string _execError = "";
        //
        // protected string DisplayError {
        //     get
        //     {
        //         if (_error != "" && _execError != "")
        //         {
        //             return $"{_error}\n\n{_execError}";
        //         }
        //         return $"{_error}{_execError}";
        //     }
        // }

        protected readonly RichTextDrawer RichTextDrawer = new RichTextDrawer();

        ~DecToggleAttributeDrawer()
        {
            RichTextDrawer.Dispose();
        }

        // private const BindingFlags BindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
        //                                                   BindingFlags.Public | BindingFlags.DeclaredOnly;

        protected Rect Draw(Rect position, SerializedProperty property, GUIContent label, string labelXml, bool isActive, Action<bool> activeCallback)
        {
            (Rect buttonRect, Rect leftRect) = RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);

            object target = property.serializedObject.targetObject;
            Type objType = target.GetType();
            // string buttonLabelXml = GetButtonLabelXml(aboveButtonAttribute, target, objType);

            GUIStyle style = new GUIStyle("Button")
            {
                // fixedWidth = btnWidth,
            };

            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                bool newIsActive = GUI.Toggle(position, isActive, "", style);
                if (changed.changed)
                {
                    activeCallback(newIsActive);
                }
            }

            // if (GUI.Button(buttonRect, string.Empty))
            // {
            //     _execError = "";
            //     MethodInfo callMethodInfo = objType.GetMethod(aboveButtonAttribute.FuncName, BindAttr);
            //
            //     if (callMethodInfo == null)
            //     {
            //         _execError = $"No field or method named `{aboveButtonAttribute.FuncName}` found on `{target}`";
            //     }
            //
            //     else
            //     {
            //         ParameterInfo[] methodParams = callMethodInfo.GetParameters();
            //         Debug.Assert(methodParams.All(p => p.IsOptional));
            //         // Debug.Assert(methodInfo.ReturnType == typeof(bool));
            //         // Debug.Log($"call {callMethodInfo}/{aboveButtonAttribute.FuncName}");
            //         try
            //         {
            //             callMethodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
            //         }
            //         catch (TargetInvocationException e)
            //         {
            //             _execError = e.InnerException!.Message;
            //             Debug.LogException(e);
            //         }
            //         catch (Exception e)
            //         {
            //             _execError = e.Message;
            //             Debug.LogException(e);
            //         }
            //     }
            // }

            // GetWidth

            IReadOnlyList<RichTextDrawer.RichTextChunk> richChunks = RichTextDrawer.ParseRichXml(labelXml, label.text).ToArray();
            float textWidth = RichTextDrawer.GetWidth(label, buttonRect.height, richChunks);
            Rect labelRect = buttonRect;
            if (textWidth < labelRect.width)
            {
                float space = (labelRect.width - textWidth) / 2f;
                labelRect.x += space;
            }
            RichTextDrawer.DrawChunks(labelRect, label, richChunks);

            return leftRect;
        }

        protected string GetButtonLabelXmlCallback(string name, object target, Type objType)
        {
            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                ReflectUtils.GetProp(objType, name);
            switch (getPropType)
            {
                case ReflectUtils.GetPropType.NotFound:
                {
                    _error = $"No field or method named `{name}` found on `{target}`";
                    return name;
                }
                case ReflectUtils.GetPropType.Field:
                {
                    FieldInfo findFieldInfo = (FieldInfo)fieldOrMethodInfo;
                    object value = findFieldInfo.GetValue(target);
                    return value == null ? string.Empty : value.ToString();
                }
                case ReflectUtils.GetPropType.Method:
                {
                    MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    // Debug.Assert(methodInfo.ReturnType == typeof(string));
                    if (methodInfo.ReturnType != typeof(string))
                    {
                        _error =
                            $"Return type of callback method `{name}` should be string";
                        return name;
                    }

                    _error = "";
                    return
                        (string)methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            }
        }
    }
}
