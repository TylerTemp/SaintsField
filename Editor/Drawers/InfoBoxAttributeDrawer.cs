using System;
using System.Collections.Generic;
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
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    [CustomPropertyDrawer(typeof(BelowInfoBoxAttribute))]
    public class InfoBoxAttributeDrawer: SaintsPropertyDrawer
    {
        #region IMGUI
        private string _error = "";

        // private bool _overrideMessageType;
        // private EMessageType _messageType;

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if(infoboxAttribute.Below)
            {
                return false;
            }

            MetaInfo metaInfo = GetMetaInfo(property, infoboxAttribute, info, parent);

            // (string error, bool willDraw) = WillDraw(infoboxAttribute, parent);
            _error = metaInfo.Error;
            return metaInfo.WillDrawBox;
        }

        protected override float GetAboveExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;
            if (infoboxAttribute.Below)
            {
                return 0;
            }

            MetaInfo metaInfo = GetMetaInfo(property, infoboxAttribute, info, parent);
            if (metaInfo.Error != "")
            {
                return 0;
            }

            return ImGuiHelpBox.GetHeight(metaInfo.Content, width, metaInfo.MessageType);
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;
            MetaInfo metaInfo = GetMetaInfo(property, infoboxAttribute, info, parent);
            return ImGuiHelpBox.Draw(position, metaInfo.Content, metaInfo.MessageType);
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            if (_error != "")
            {
                return true;
            }

            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if(!infoboxAttribute.Below)
            {
                return false;
            }

            MetaInfo metaInfo = GetMetaInfo(property, infoboxAttribute, info, parent);
            _error = metaInfo.Error;
            return metaInfo.WillDrawBox;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;

            float boxHeight = 0f;
            if (infoboxAttribute.Below)
            {
                MetaInfo metaInfo = GetMetaInfo(property, infoboxAttribute, info, parent);
                _error = metaInfo.Error;
                if (metaInfo.WillDrawBox)
                {
                    boxHeight = ImGuiHelpBox.GetHeight(metaInfo.Content, width, metaInfo.MessageType);
                }
            }

            float errorHeight = _error != "" ? ImGuiHelpBox.GetHeight(_error, width, EMessageType.Error) : 0f;
            return boxHeight + errorHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;
            MetaInfo metaInfo = GetMetaInfo(property, infoboxAttribute, info, parent);
            Rect leftRect = ImGuiHelpBox.Draw(position, metaInfo.Content, metaInfo.MessageType);
            _error = metaInfo.Error;

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (_error == "")
            {
                return leftRect;
            }

            return ImGuiHelpBox.Draw(leftRect, _error, EMessageType.Error);
        }

        #endregion

        private struct MetaInfo
        {
            // ReSharper disable InconsistentNaming
            public string Error;
            public EMessageType MessageType;
            public string Content;
            public bool WillDrawBox;
            // ReSharper enable InconsistentNaming
        }

        private static MetaInfo GetMetaInfo(SerializedProperty property, InfoBoxAttribute infoboxAttribute,
            FieldInfo info, object target)
        {
            (string drawError, bool willDraw) = WillDraw(property, infoboxAttribute, info, target);
            if (drawError != "")
            {
                return new MetaInfo
                {
                    Error = drawError,
                };
            }

            if (!infoboxAttribute.IsCallback)
            {
                return new MetaInfo
                {
                    Content = infoboxAttribute.Content,
                    MessageType = infoboxAttribute.MessageType,
                    WillDrawBox = infoboxAttribute.Content != null && willDraw,
                    Error = "",
                };
            }

            (string error, object result) = Util.GetOf<object>(
                infoboxAttribute.Content,
                null,
                property,
                info,
                target);

            if (error != "")
            {
                return new MetaInfo
                {
                    Error = error,
                };
            }

            if (result is ValueTuple<EMessageType, string> resultTuple)
            {
                return new MetaInfo
                {
                    Error = "",
                    WillDrawBox = resultTuple.Item2 != null && willDraw,
                    MessageType = resultTuple.Item1,
                    Content = resultTuple.Item2,
                };
            }

            // Debug.Log($"result={result}, null={result == null}, willDraw={willDraw}/{property.propertyPath}/{info.Name}");

            return new MetaInfo
            {
                Error = "",
                WillDrawBox = result != null && willDraw,
                MessageType = infoboxAttribute.MessageType,
                Content = result == null ? "" : result.ToString(),
            };
        }

        private static (string error, bool willDraw) WillDraw(SerializedProperty property, InfoBoxAttribute infoboxAttribute, FieldInfo info, object target)
        {
            if (infoboxAttribute.ShowCallback == null)
            {
                return ("", true);
            }

            (string error, object result) = Util.GetOf<object>(
                infoboxAttribute.ShowCallback,
                null,
                property,
                info,
                target);

            return error != ""
                ? (error, false)
                : ("", ReflectUtils.Truly(result));
        }

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameInfoBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__InfoBox";
        private static string NameHelpBox(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__InfoBoxHelpBox";

        protected override VisualElement CreateAboveUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;
            if (infoboxAttribute.Below)
            {
                return null;
            }

            return new HelpBox
            {
                name = NameInfoBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
                userData = new MetaInfo
                {
                    Content = "",
                    Error = "",
                    MessageType = EMessageType.None,
                },
            };
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container,
            FieldInfo info,
            object parent)
        {
            HelpBox errorBox = new HelpBox
            {
                name = NameHelpBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
            };

            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;
            if(!infoboxAttribute.Below)
            {
                return errorBox;
            }

            VisualElement root = new VisualElement();

            root.Add(new HelpBox
            {
                name = NameInfoBox(property, index),
                style =
                {
                    display = DisplayStyle.None,
                },
                userData = new MetaInfo
                {
                    Content = "",
                    Error = "",
                    MessageType = EMessageType.None,
                },
            });
            root.Add(errorBox);

            root.AddToClassList(ClassAllowDisable);

            // OnUpdateUiToolKit(property, saintsAttribute, index, root, parent);
            return root;
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChangedCallback, FieldInfo info)
        {
            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            MetaInfo metaInfo = GetMetaInfo(property, (InfoBoxAttribute)saintsAttribute, info, parent);
            HelpBox infoBox = container.Q<HelpBox>(NameInfoBox(property, index));
            MetaInfo oriMetaInfo = (MetaInfo)infoBox.userData;

            bool changed = false;
            if (oriMetaInfo.Error != metaInfo.Error)
            {
                changed = true;
                HelpBox errorBox = container.Q<HelpBox>(NameHelpBox(property, index));
                errorBox.style.display = metaInfo.Error != "" ? DisplayStyle.Flex : DisplayStyle.None;
                errorBox.text = metaInfo.Error;
            }

            if(oriMetaInfo.MessageType != metaInfo.MessageType || oriMetaInfo.Content != metaInfo.Content || oriMetaInfo.WillDrawBox != metaInfo.WillDrawBox)
            {

                // Debug.Log($"change box {NameInfoBox(property, index)}: {metaInfo.MessageType} {metaInfo.Content} {metaInfo.WillDraw}");
                changed = true;
                infoBox.style.display = metaInfo.WillDrawBox ? DisplayStyle.Flex : DisplayStyle.None;
                infoBox.text = metaInfo.Content;
                infoBox.messageType = metaInfo.MessageType.GetUIToolkitMessageType();
            }

            if (changed)
            {
                infoBox.userData = metaInfo;
            }
        }

        #endregion

#endif
    }
}
