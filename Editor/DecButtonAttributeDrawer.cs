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
    public abstract class DecButtonAttributeDrawer: SaintsPropertyDrawer
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

        ~DecButtonAttributeDrawer()
        {
            _richTextDrawer.Dispose();
        }

        protected float GetExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute)
        {
            float result = EditorGUIUtility.singleLineHeight + (DisplayError == ""? 0: HelpBox.GetHeight(DisplayError, width));
            // Debug.Log($"AboveButtonAttributeDrawer.GetAboveExtraHeight={result}/{DisplayError}");
            return result;
        }

        protected Rect Draw(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            // Debug.Log(DisplayError);
            DecButtonAttribute aboveButtonAttribute = (DecButtonAttribute) saintsAttribute;

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
                (ReflectUil.GetPropType getPropType, object fieldOrMethodInfo) = ReflectUil.GetProp(objType, aboveButtonAttribute.ButtonLabel);
                switch (getPropType)
                {
                    case ReflectUil.GetPropType.NotFound:
                    {
                        _error = $"No field or method named `{aboveButtonAttribute.ButtonLabel}` found on `{target}`";
                        buttonLabelXml = aboveButtonAttribute.ButtonLabel;
                    }
                        break;
                    case ReflectUil.GetPropType.Field:
                    {
                        FieldInfo findFieldInfo = (FieldInfo)fieldOrMethodInfo;
                        object value = findFieldInfo.GetValue(target);
                        buttonLabelXml = value == null ? string.Empty : value.ToString();
                    }
                        break;
                    case ReflectUil.GetPropType.Method:
                    {
                        MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                        ParameterInfo[] methodParams = methodInfo.GetParameters();
                        Debug.Assert(methodParams.All(p => p.IsOptional));
                        // Debug.Assert(methodInfo.ReturnType == typeof(string));
                        if (methodInfo.ReturnType != typeof(string))
                        {
                            _error =
                                $"Return type of callback method `{aboveButtonAttribute.ButtonLabel}` should be string";
                            buttonLabelXml = aboveButtonAttribute.ButtonLabel;
                        }
                        else
                        {
                            _error = "";
                            buttonLabelXml =
                                (string)methodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
                        }
                    }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
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
                    // return leftRect;
                }

                else
                {
                    ParameterInfo[] methodParams = callMethodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    // Debug.Assert(methodInfo.ReturnType == typeof(bool));
                    Debug.Log($"call {callMethodInfo}/{aboveButtonAttribute.FuncName}");
                    try
                    {
                        callMethodInfo.Invoke(target, methodParams.Select(p => p.DefaultValue).ToArray());
                    }
                    catch (TargetInvocationException e)
                    {
                        _execError = e.InnerException!.Message;
                        Debug.Log($"execError set to {_execError}");
                        Debug.LogException(e);
                    }
                    catch (Exception e)
                    {
                        _execError = e.Message;
                        Debug.Log($"execError set to {_execError}");
                        Debug.LogException(e);
                    }
                }
            }

            _richTextDrawer.DrawChunks(buttonRect, label, RichTextDrawer.ParseRichXml(buttonLabelXml, label.text));

            if (DisplayError != "")
            {
                HelpBox.Draw(errorRect, DisplayError, MessageType.Error);
            }

            // Debug.Log(DisplayError);
            // Debug.Log(_execError);

            return leftRect;

        }
    }
}
