using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    public abstract class DecButtonAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";
        private string _execError = "";

        protected string DisplayError {
            get
            {
                if (_error != "" && _execError != "")
                {
                    return $"{_error}\n\n{_execError}";
                }
                return $"{_error}{_execError}";
            }
        }

        protected readonly RichTextDrawer RichTextDrawer = new RichTextDrawer();
        // private IReadOnlyList<RichText.RichTextPayload> _cachedResult = null;

        ~DecButtonAttributeDrawer()
        {
            RichTextDrawer.Dispose();
        }

        // protected float GetExtraHeight(SerializedProperty property, GUIContent label,
        //     float width,
        //     ISaintsAttribute saintsAttribute)
        // {
        //     float result = EditorGUIUtility.singleLineHeight + (DisplayError == ""? 0: HelpBox.GetHeight(DisplayError, width));
        //     // Debug.Log($"AboveButtonAttributeDrawer.GetAboveExtraHeight={result}/{DisplayError}");
        //     return result;
        // }

        private const BindingFlags BindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                                          BindingFlags.Public | BindingFlags.DeclaredOnly;

        protected Rect Draw(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            // Debug.Log(DisplayError);
            DecButtonAttribute aboveButtonAttribute = (DecButtonAttribute) saintsAttribute;

            (Rect buttonRect, Rect leftRect) = RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);
            // Rect errorRect = new Rect(leftRect)
            // {
            //     height = 0,
            // };
            // if (DisplayError != "")
            // {
            //     (Rect errorRectCut, Rect leftRectCut) = RectUtils.SplitHeightRect(leftRect, HelpBox.GetHeight(DisplayError, position.width));
            //     errorRect = errorRectCut;
            //     leftRect = leftRectCut;
            // }

            object target = property.serializedObject.targetObject;
            Type objType = target.GetType();
            string buttonLabelXml = GetButtonLabelXml(aboveButtonAttribute, target, objType);
            // const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
            //                               BindingFlags.Public | BindingFlags.DeclaredOnly;


            if (GUI.Button(buttonRect, string.Empty))
            {
                _execError = "";
                MethodInfo callMethodInfo = objType.GetMethod(aboveButtonAttribute.FuncName, BindAttr);
                // if (callMethodInfo == null)
                // {
                //     callMethodInfo = objType.GetMethod($"<{aboveButtonAttribute.FuncName}>k__BackingField", bindAttr);
                // }

                if (callMethodInfo == null)
                {
                    _execError = $"No field or method named `{aboveButtonAttribute.FuncName}` found on `{target}`";
                    // return leftRect;
                }

                else
                {
                    ParameterInfo[] methodParams = callMethodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    // Debug.Assert(methodInfo.ReturnType == typeof(bool));
                    // Debug.Log($"call {callMethodInfo}/{aboveButtonAttribute.FuncName}");
                    try
                    {
                        callMethodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
                    }
                    catch (TargetInvocationException e)
                    {
                        _execError = e.InnerException!.Message;
                        Debug.LogException(e);
                    }
                    catch (Exception e)
                    {
                        _execError = e.Message;
                        Debug.LogException(e);
                    }
                }
            }


            // GetWidth
            IReadOnlyList<RichTextDrawer.RichTextChunk> richChunks = RichTextDrawer.ParseRichXml(buttonLabelXml, label.text).ToArray();
            float textWidth = RichTextDrawer.GetWidth(label, buttonRect.height, richChunks);
            Rect labelRect = buttonRect;
            // Debug.Log($"textWidth={textWidth}, labelRect.width={labelRect.width}");
            if (textWidth < labelRect.width)
            {
                float space = (labelRect.width - textWidth) / 2f;
                labelRect.x += space;
            }
            RichTextDrawer.DrawChunks(labelRect, label, richChunks);

            // if (DisplayError != "")
            // {
            //     HelpBox.Draw(errorRect, DisplayError, MessageType.Error);
            // }

            // Debug.Log(DisplayError);
            // Debug.Log(_execError);

            return leftRect;

        }

        protected string GetButtonLabelXml(DecButtonAttribute decButtonAttribute, object target, Type objType)
        {
            if (!decButtonAttribute.ButtonLabelIsCallback)
            {
                return decButtonAttribute.ButtonLabel;
            }

            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                ReflectUtils.GetProp(objType, decButtonAttribute.ButtonLabel);
            switch (getPropType)
            {
                case ReflectUtils.GetPropType.NotFound:
                {
                    _error = $"No field or method named `{decButtonAttribute.ButtonLabel}` found on `{target}`";
                    return decButtonAttribute.ButtonLabel;
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
                            $"Return type of callback method `{decButtonAttribute.ButtonLabel}` should be string";
                        return decButtonAttribute.ButtonLabel;
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
