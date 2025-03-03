using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.InfoBoxDrawer
{
    public partial class InfoBoxAttributeDrawer
    {
        private string _error = "";

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            InfoBoxAttribute infoboxAttribute = (InfoBoxAttribute)saintsAttribute;

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (infoboxAttribute.Below)
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

            if (!metaInfo.WillDrawBox)
            {
                return 0;
            }

            // Debug.Log($"above {metaInfo.Content}");

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
            if (!infoboxAttribute.Below)
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
            // Debug.Log($"below {boxHeight} + {errorHeight}");
            return boxHeight + errorHeight;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
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
    }
}
