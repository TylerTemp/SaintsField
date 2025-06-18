#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.InfoBoxDrawer
{
    public partial class InfoBoxAttributeDrawer
    {
        #region UIToolkit

        private static string NameInfoBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__InfoBox";

        private static string NameHelpBox(SerializedProperty property, int index) =>
            $"{property.propertyPath}_{index}__InfoBoxHelpBox";

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
            IReadOnlyList<PropertyAttribute> allAttributes,
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
            if (!infoboxAttribute.Below)
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
            IReadOnlyList<PropertyAttribute> allAttributes,
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

            if (oriMetaInfo.MessageType != metaInfo.MessageType || oriMetaInfo.Content != metaInfo.Content ||
                oriMetaInfo.WillDrawBox != metaInfo.WillDrawBox)
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
    }
}
#endif
