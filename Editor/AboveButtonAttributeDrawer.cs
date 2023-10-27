using System;
using System.Linq;
using System.Reflection;
using ExtInspector.Editor.Standalone;
using ExtInspector.Editor.Utils;
using ExtInspector.Standalone;
using UnityEditor;
using UnityEngine;

namespace ExtInspector.Editor
{
    [CustomPropertyDrawer(typeof(AboveButtonAttribute))]
    public class AboveButtonAttributeDrawer: SaintsPropertyDrawer
    {
        private string _error = "";
        private string _execError = "";

        private string DisplayError {
            get
            {
                if (_error != "" && _execError != "")
                {
                    return $"{_error}\n\n{_execError}";
                }
                return $"{_error}{_execError}";
            }
        }

        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        // private IReadOnlyList<RichText.RichTextPayload> _cachedResult = null;

        ~AboveButtonAttributeDrawer()
        {
            _richTextDrawer.Dispose();
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute)
        {
            float result = EditorGUIUtility.singleLineHeight + (DisplayError == ""? 0: HelpBox.GetHeight(DisplayError, width));
            // Debug.Log($"AboveButtonAttributeDrawer.GetAboveExtraHeight={result}/{DisplayError}");
            return result;
        }

        protected override bool WillDrawAbove(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return true;
        }

        protected override Rect DrawAbove(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            AboveButtonAttribute aboveButtonAttribute = (AboveButtonAttribute) saintsAttribute;

            (Rect buttonRect, Rect leftRect) = RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);
            Rect errorRect = new Rect(leftRect)
            {
                height = 0,
            };
            if (DisplayError != "")
            {
                (Rect errorRectCut, Rect leftRectCut) = RectUtils.SplitHeightRect(leftRect, HelpBox.GetHeight(DisplayError, position.width));
                errorRect = errorRectCut;
                leftRect = leftRectCut;
            }

            string buttonLabelXml = aboveButtonAttribute.ButtonLabel;
            object target = property.serializedObject.targetObject;
            Type objType = target.GetType();
            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly;
            if (aboveButtonAttribute.ButtonLabelIsCallback)
            {
                MethodInfo methodInfo = objType.GetMethod(aboveButtonAttribute.ButtonLabel, bindAttr);
                if (methodInfo == null)
                {
                    methodInfo = objType.GetMethod($"<{aboveButtonAttribute.ButtonLabel}>k__BackingField", bindAttr);
                }

                if (methodInfo == null)
                {
                    _error = $"No field or method named `{aboveButtonAttribute.ButtonLabel}` found on `{target}`";
                    buttonLabelXml = aboveButtonAttribute.ButtonLabel;
                }
                else
                {
                    _error = "";
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    // Debug.Assert(methodInfo.ReturnType == typeof(string));
                    if (methodInfo.ReturnType != typeof(string))
                    {
                        _error = $"Return type of callback method `{aboveButtonAttribute.ButtonLabel}` should be string";
                        buttonLabelXml = aboveButtonAttribute.ButtonLabel;
                    }
                    else
                    {
                        buttonLabelXml =
                            (string)methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
                    }
                }
            }

            if (GUI.Button(buttonRect, string.Empty))
            {
                _execError = "";
                MethodInfo callMethodInfo = objType.GetMethod(aboveButtonAttribute.FuncName, bindAttr);
                if (callMethodInfo == null)
                {
                    callMethodInfo = objType.GetMethod($"<{aboveButtonAttribute.FuncName}>k__BackingField", bindAttr);
                }

                if (callMethodInfo == null)
                {
                    _execError = $"No field or method named `{aboveButtonAttribute.FuncName}` found on `{target}`";
                    buttonLabelXml = label.text;
                }
                else
                {
                    ParameterInfo[] methodParams = callMethodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    // Debug.Assert(methodInfo.ReturnType == typeof(bool));
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

            _richTextDrawer.DrawChunks(buttonRect, label, RichTextDrawer.ParseRichXml(buttonLabelXml, label.text));

            if (DisplayError != "")
            {
                HelpBox.Draw(errorRect, DisplayError, MessageType.Error);
            }

            // if (aboveButtonAttribute.ButtonLabelIsCallback)
            // {
            //     if (GUI.Button(buttonRect, aboveButtonAttribute.ButtonLabel))
            //     {
            //         property.serializedObject.targetObject.GetType().GetMethod(aboveButtonAttribute.FuncName)!.Invoke(property.serializedObject.targetObject, null);
            //     }
            // }
            // else
            // {
            //     if (GUI.Button(buttonRect, aboveButtonAttribute.ButtonLabel))
            //     {
            //         property.serializedObject.targetObject.GetType().GetMethod(aboveButtonAttribute.FuncName)!.Invoke(property.serializedObject.targetObject, new object[] {property});
            //     }
            // }

            return leftRect;

        }
    }
}
