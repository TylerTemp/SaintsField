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
        #region IMGUI
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        private string _error = "";

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            _richTextDrawer.Dispose();
        }

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            (string error, string xml) =
                RichTextDrawer.GetLabelXml(property, fullWidthRichLabelAttribute.RichTextXml, fullWidthRichLabelAttribute.IsCallback, info, parent);
            if (error != "")
            {
                _error = error;
            }

            if (string.IsNullOrEmpty(xml))
            {
                return false;
            }

            return fullWidthRichLabelAttribute.Above;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            if (!fullWidthRichLabelAttribute.Above)
            {
                return 0;
            }

            (string error, string xml) =
                RichTextDrawer.GetLabelXml(property, fullWidthRichLabelAttribute.RichTextXml, fullWidthRichLabelAttribute.IsCallback, info, parent);
            if (error != "")
            {
                _error = error;
            }
            return string.IsNullOrEmpty(xml)? 0 : EditorGUIUtility.singleLineHeight;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            if (!fullWidthRichLabelAttribute.Above)
            {
                return position;
            }

            return DrawImGui(position, property, label, saintsAttribute, info, parent);
        }

        private Rect DrawImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;

            (string error, string xml) =
                RichTextDrawer.GetLabelXml(property, fullWidthRichLabelAttribute.RichTextXml, fullWidthRichLabelAttribute.IsCallback, info, parent);
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

            string labelText = label.text;
#if SAINTSFIELD_NAUGHYTATTRIBUTES
            labelText = property.displayName;
#endif

            ImGuiEnsureDispose(property.serializedObject.targetObject);
            _richTextDrawer.DrawChunks(curRect, label, RichTextDrawer.ParseRichXml(xml, labelText, info, parent));
            return leftRect;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            (string error, string xml) =
                RichTextDrawer.GetLabelXml(property, fullWidthRichLabelAttribute.RichTextXml, fullWidthRichLabelAttribute.IsCallback, info, parent);
            if (error != "")
            {
                _error = error;
            }

            if (_error != "")
            {
                return true;
            }

            if (fullWidthRichLabelAttribute.Above)
            {
                return false;
            }

            return !string.IsNullOrEmpty(xml);
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            float xmlHeight = 0;
            if (!fullWidthRichLabelAttribute.Above)
            {
                (string error, string xml) =
                    RichTextDrawer.GetLabelXml(property, fullWidthRichLabelAttribute.RichTextXml, fullWidthRichLabelAttribute.IsCallback, info, parent);
                if (error != "")
                {
                    _error = error;
                }

                xmlHeight = string.IsNullOrEmpty(xml) ? 0 : EditorGUIUtility.singleLineHeight;
            }
            float errorHeight = _error == "" ? 0 : ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
            // Debug.Log($"#FullWidthRichLabel# below height={errorHeight}+{xmlHeight}/property={property.propertyPath}");

            return errorHeight + xmlHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            // EditorGUI.DrawRect(position, Color.green);
            Rect useRect = position;
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            if (!fullWidthRichLabelAttribute.Above)
            {
                useRect = DrawImGui(position, property, label, fullWidthRichLabelAttribute, info, parent);
            }
            return _error == ""
                ? useRect
                : ImGuiHelpBox.Draw(useRect, _error, MessageType.Error);
        }

        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UI Toolkit

        private static string NameFullWidthLabelContainer(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__FullWidthRichLabel_Container";
        private static string NameHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__FullWidthRichLabel_HelpBox";

        private static VisualElement DrawUIToolKit(SerializedProperty property, int index)
        {
            // FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            //
            // (string error, string xml) = GetLabelXml(fullWidthRichLabelAttribute, parent);
            //
            // if (labelXml is null)
            // {
            //     return new VisualElement();
            // }

            VisualElement visualElement = new VisualElement
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
            visualElement.AddToClassList(ClassAllowDisable);
            return visualElement;
        }

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            return fullWidthRichLabelAttribute.Above
                ? DrawUIToolKit(property, index)
                : null;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement();

            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute) saintsAttribute;
            if (!fullWidthRichLabelAttribute.Above)
            {
                root.Add(DrawUIToolKit(property, index));
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

            root.AddToClassList(ClassAllowDisable);

            return root;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            FullWidthRichLabelAttribute fullWidthRichLabelAttribute = (FullWidthRichLabelAttribute)saintsAttribute;
            (string error, string xml) = RichTextDrawer.GetLabelXml(property, fullWidthRichLabelAttribute.RichTextXml, fullWidthRichLabelAttribute.IsCallback, info, parent);

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
                    foreach (VisualElement rich in _richTextDrawer.DrawChunksUIToolKit(RichTextDrawer.ParseRichXml(xml, property.displayName, info, parent)))
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
