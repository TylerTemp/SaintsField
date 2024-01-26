using System;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(AboveRichLabelAttribute))]
    [CustomPropertyDrawer(typeof(BelowRichLabelAttribute))]
    [CustomPropertyDrawer(typeof(FullWidthRichLabelAttribute))]
    public class FullWidthRichLabelAttributeDrawer: SaintsPropertyDrawer
    {
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        private string _error = "";

        ~FullWidthRichLabelAttributeDrawer()
        {
            _richTextDrawer.Dispose();
        }

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            object parent)
        {
            // Debug.Log("ABOVE!");
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            return fullWidthRichLabelAttribute.Above;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            return fullWidthRichLabelAttribute.Above? EditorGUIUtility.singleLineHeight: 0;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, object parent)
        {
            return DrawImGui(position, label, saintsAttribute, parent);
        }

        private Rect DrawImGui(Rect position, GUIContent label, ISaintsAttribute saintsAttribute, object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;

            (string error, string xml) = GetLabelXml(fullWidthRichLabelAttribute, parent);
            if(error != "")
            {
                _error = error;
                return position;
            }

            if (xml is null)
            {
                return position;
            }

            (Rect curRect, Rect leftRect) = RectUtils.SplitHeightRect(position, EditorGUIUtility.singleLineHeight);

            _richTextDrawer.DrawChunks(curRect, label, RichTextDrawer.ParseRichXml(xml, label.text));
            return leftRect;
        }

        private static (string error, string xml) GetLabelXml(FullWidthRichLabelAttribute targetAttribute, object target)
        {
            if (!targetAttribute.IsCallback)
            {
                return ("", targetAttribute.RichTextXml);
            }

            (ReflectUtils.GetPropType getPropType, object fieldOrMethodInfo) =
                ReflectUtils.GetProp(target.GetType(), targetAttribute.RichTextXml);
            switch (getPropType)
            {
                case ReflectUtils.GetPropType.Field:
                {
                    object result = ((FieldInfo)fieldOrMethodInfo).GetValue(target);
                    return ("", result == null ? string.Empty : result.ToString());
                }

                case ReflectUtils.GetPropType.Property:
                {
                    object result = ((PropertyInfo)fieldOrMethodInfo).GetValue(target);
                    return ("", result == null ? string.Empty : result.ToString());
                }
                case ReflectUtils.GetPropType.Method:
                {
                    MethodInfo methodInfo = (MethodInfo)fieldOrMethodInfo;
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    Debug.Assert(methodParams.All(p => p.IsOptional));
                    if (methodInfo.ReturnType != typeof(string))
                    {
                        return (
                            $"Expect returning string from `{targetAttribute.RichTextXml}`, get {methodInfo.ReturnType}",
                            "");
                    }

                    try
                    {
                        string xml = (string)methodInfo.Invoke(target,
                            methodParams.Select(p => p.DefaultValue).ToArray());
                        return ("", xml);
                    }
                    catch (TargetInvocationException e)
                    {
                        Debug.LogException(e);
                        Debug.Assert(e.InnerException != null);
                        return (e.InnerException.Message, null);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        return (e.Message, null);
                    }
                }
                case ReflectUtils.GetPropType.NotFound:
                {
                    return ($"not found `{targetAttribute.RichTextXml}` on `{target}`", null);
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(getPropType), getPropType, null);
            }
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            return !fullWidthRichLabelAttribute.Above || _error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            float errorHeight = _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
            return fullWidthRichLabelAttribute.Above
                ? errorHeight
                : errorHeight + EditorGUIUtility.singleLineHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            Rect useRect = position;
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            if (!fullWidthRichLabelAttribute.Above)
            {
                useRect = DrawImGui(position, label, fullWidthRichLabelAttribute, parent);
            }
            return _error == ""
                ? useRect
                : ImGuiHelpBox.Draw(useRect, _error, MessageType.Error);
        }

#if UNITY_2021_3_OR_NEWER

        #region UI Toolkit

        private static string NameFullWidthLabelContainer(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__FullWidthRichLabel_Container";
        private static string NameHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__FullWidthRichLabel_HelpBox";

        private VisualElement DrawUIToolKit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, object parent)
        {
            // FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            //
            // (string error, string xml) = GetLabelXml(fullWidthRichLabelAttribute, parent);
            //
            // if (labelXml is null)
            // {
            //     return new VisualElement();
            // }

            return new VisualElement
            {
                style =
                {
                    // height = EditorGUIUtility.singleLineHeight,
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                    marginLeft = 4,
                    // textOverflow = TextOverflow.wr
                    // alignItems = Align.Center, // vertical
                    // overflow = Overflow.Hidden,
                    // overflow = Overflow.
                },
                pickingMode = PickingMode.Ignore,
                name = NameFullWidthLabelContainer(property, index),
                userData = null,
            };
        }

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            return fullWidthRichLabelAttribute.Above
                ? DrawUIToolKit(property, saintsAttribute, index, parent)
                : null;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement();

            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute) saintsAttribute;
            if (!fullWidthRichLabelAttribute.Above)
            {
                root.Add(DrawUIToolKit(property, saintsAttribute, index, parent));
            }

            root.Add(new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property, index),
                userData = "",
            });
            return root;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, object parent)
        {
            (string error, string xml) = GetLabelXml((FullWidthRichLabelAttribute)saintsAttribute, parent);

            HelpBox helpBox = container.Q<HelpBox>(NameHelpBox(property, index));
            if ((string)helpBox.userData != error)
            {
                helpBox.userData = error;
                helpBox.text = error;
                helpBox.style.display = string.IsNullOrEmpty(error) ? DisplayStyle.None : DisplayStyle.Flex;
            }

            VisualElement fullWidthLabelContainer = container.Q<VisualElement>(NameFullWidthLabelContainer(property, index));
            if ((string)fullWidthLabelContainer.userData != xml)
            {
                fullWidthLabelContainer.userData = xml;
                if (string.IsNullOrEmpty(xml))
                {
                    fullWidthLabelContainer.style.display = DisplayStyle.None;
                }
                else
                {
                    fullWidthLabelContainer.Clear();
                    fullWidthLabelContainer.style.display = DisplayStyle.Flex;
                    foreach (VisualElement rich in _richTextDrawer.DrawChunksUIToolKit(RichTextDrawer.ParseRichXml(xml, property.displayName)))
                    {
                        fullWidthLabelContainer.Add(rich);
                    }
                }
            }
        }

        #endregion

#endif
    }
}
