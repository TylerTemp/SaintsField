using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(RichLabelAttribute))]
    public class RichLabelAttributeDrawer: SaintsPropertyDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        private string _error = "";

        // private readonly Color _backgroundColor;
        //
        // public RichLabelAttributeDrawer()
        // {
        //     _backgroundColor = EditorGUIUtility.isProSkin
        //         ? new Color32(56, 56, 56, 255)
        //         : new Color32(194, 194, 194, 255);
        // }

        ~RichLabelAttributeDrawer()
        {
            _richTextDrawer.Dispose();
        }

        // protected override float GetLabelHeight(SerializedProperty property, GUIContent label,
        //     ISaintsAttribute saintsAttribute) =>
        //     EditorGUIUtility.singleLineHeight;

        protected override bool WillDrawLabel(SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            RichLabelAttribute targetAttribute = (RichLabelAttribute)saintsAttribute;
            bool result = GetLabelXml(property, targetAttribute) != null;
            // Debug.Log($"richLabel willDraw={result}");
            return result;
        }

        protected override void DrawLabel(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute)
        {
            RichLabelAttribute targetAttribute = (RichLabelAttribute)saintsAttribute;

            string labelXml = GetLabelXml(property, targetAttribute);

            if (labelXml is null)
            {
                return;
            }

            // EditorGUI.DrawRect(position, _backgroundColor);
            _richTextDrawer.DrawChunks(position, label, RichTextDrawer.ParseRichXml(labelXml, label.text));
            // LabelMouseProcess(position, property);
        }

        private string GetLabelXml(SerializedProperty property, RichLabelAttribute targetAttribute)
        {
            if (!targetAttribute.IsCallback)
            {
                return targetAttribute.RichTextXml;
            }

            _error = "";
            object target = property.serializedObject.targetObject;
            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                ReflectUtils.GetProp(target.GetType(), targetAttribute.RichTextXml);
            switch (getPropType)
            {
                case ReflectUtils.GetPropType.Field:
                {
                    object result = ((FieldInfo)fieldOrMethodInfo).GetValue(target);
                    return result == null ? string.Empty : result.ToString();
                }

                case ReflectUtils.GetPropType.Property:
                {
                    object result = ((PropertyInfo)fieldOrMethodInfo).GetValue(target);
                    return result == null ? string.Empty : result.ToString();
                }
                case ReflectUtils.GetPropType.Method:
                {
                    MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    if (methodInfo.ReturnType != typeof(string))
                    {
                        _error =
                            $"Expect returning string from `{targetAttribute.RichTextXml}`, get {methodInfo.ReturnType}";
                    }
                    else
                    {
                        try
                        {
                            return (string)methodInfo.Invoke(target,
                                methodParams.Select(p => p.DefaultValue).ToArray());
                        }
                        catch (TargetInvocationException e)
                        {
                            _error = e.InnerException!.Message;
                            Debug.LogException(e);
                        }
                        catch (Exception e)
                        {
                            _error = e.Message;
                            Debug.LogException(e);
                        }
                    }
                    return null;
                }
                case ReflectUtils.GetPropType.NotFound:
                {
                    _error =
                        $"not found `{targetAttribute.RichTextXml}` on `{target}`";
                    return null;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            }
        }

        protected override bool WillDrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width, ISaintsAttribute saintsAttribute)
        {
            return _error == "" ? 0 : HelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label, ISaintsAttribute saintsAttribute)
        {
            return HelpBox.Draw(position, _error, MessageType.Error);
        }
    }
}
