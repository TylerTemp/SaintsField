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

        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        // private IReadOnlyList<RichText.RichTextPayload> _cachedResult = null;

        ~AboveButtonAttributeDrawer()
        {
            _richTextDrawer.Dispose();
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            return EditorGUIUtility.singleLineHeight + (_error == ""? 0: HelpBox.GetHeight(_error));
        }

        protected override bool WillDrawAbove(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return true;
        }

        protected override Rect DrawAbove(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            AboveButtonAttribute aboveButtonAttribute = (AboveButtonAttribute) saintsAttribute;

            (Rect buttonRect, Rect leftRect) = RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);
            Rect errorRect = new Rect(buttonRect)
            {
                y = buttonRect.y + buttonRect.height,
                height = 0,
            };
            if (_error == "")
            {
                (Rect errorRectCut, Rect leftRectCut) = RectUtils.SplitHeightRect(position, HelpBox.GetHeight(_error));
                errorRect = errorRectCut;
                leftRect = leftRectCut;
            }
            else
            {
                buttonRect = position;
            }

            string buttonLabelXml = aboveButtonAttribute.ButtonLabel;
            object target = property.serializedObject.targetObject;
            Type objType = target.GetType();
            const BindingFlags bindAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic |
                                          BindingFlags.Public | BindingFlags.DeclaredOnly;
            if (aboveButtonAttribute.ButtonLabelIsCallback)
            {
                MethodInfo methodInfo = objType.GetMethod(aboveButtonAttribute.FuncName, bindAttr);
                if (methodInfo == null)
                {
                    methodInfo = objType.GetMethod($"<{aboveButtonAttribute.FuncName}>k__BackingField", bindAttr);
                }

                if (methodInfo == null)
                {
                    _error = $"No field or method named `{aboveButtonAttribute.FuncName}` found on `{target}`";
                    buttonLabelXml = label.text;
                }
                else
                {
                    _error = "";
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    Debug.Assert(methodInfo.ReturnType == typeof(bool));
                    buttonLabelXml = (string)methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
                }
            }

            if (GUI.Button(buttonRect, string.Empty))
            {
                _execError = "";
                MethodInfo methodInfo = objType.GetMethod(aboveButtonAttribute.FuncName, bindAttr);
                if (methodInfo == null)
                {
                    methodInfo = objType.GetMethod($"<{aboveButtonAttribute.FuncName}>k__BackingField", bindAttr);
                }

                if (methodInfo == null)
                {
                    _execError = $"No field or method named `{aboveButtonAttribute.FuncName}` found on `{target}`";
                    buttonLabelXml = label.text;
                }
                else
                {
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    // Debug.Assert(methodInfo.ReturnType == typeof(bool));
                    try
                    {
                        methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
                    }
                    catch (Exception e)
                    {
                        _execError = e.ToString();
                    }
                }
            }

            _richTextDrawer.DrawChunks(buttonRect, label, RichTextDrawer.ParseRichXml(buttonLabelXml, label.text));

            if (_error != "" || _execError != "")
            {
                HelpBox.Draw(errorRect, _error == ""? _execError: _error, MessageType.Error);
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
