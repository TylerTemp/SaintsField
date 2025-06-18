using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.DisabledDrawers.ReadOnlyDrawer
{
    public partial class ReadOnlyAttributeDrawer
    {

        private string _error = "";

        protected override bool WillDrawAbove(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent)
        {
            return true;
        }

        protected override Rect DrawAboveImGui(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info,
            object parent)
        {
            (string error, bool disabled) = IsDisabled(property, info, parent);
            _error = error;
            EditorGUI.BeginDisabledGroup(disabled);
            return position;
        }

        protected override void OnPropertyEndImGui(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            int saintsIndex, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            EditorGUI.EndDisabledGroup();
        }

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return _error != "";
            // return true;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            // EditorGUI.EndDisabledGroup();

            if (_error == "")
            {
                return position;
            }

            (Rect errorRect, Rect leftRect) = RectUtils.SplitHeightRect(position,
                ImGuiHelpBox.GetHeight(_error, position.width, MessageType.Error));
            ImGuiHelpBox.Draw(errorRect, _error, MessageType.Error);
            return leftRect;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            // Debug.Log("check extra height!");
            if (_error == "")
            {
                return 0;
            }

            // Debug.Log(HelpBox.GetHeight(_error));
            return ImGuiHelpBox.GetHeight(_error, width, MessageType.Error);
        }

        protected virtual (string error, bool disabled) IsDisabled(SerializedProperty property, FieldInfo info,
            object target)
        {
            List<bool> allResults = new List<bool>();

            ReadOnlyAttribute[] targetAttributes =
                SerializedUtils.GetAttributesAndDirectParent<ReadOnlyAttribute>(property).attributes;
            foreach (ReadOnlyAttribute targetAttribute in targetAttributes)
            {
                (IReadOnlyList<string> errors, IReadOnlyList<bool> boolResults) =
                    Util.ConditionChecker(targetAttribute.ConditionInfos, property, info, target);

                if (errors.Count > 0)
                {
                    return (string.Join("\n\n", errors), false);
                }

                // And Mode
                bool boolResultsOk = boolResults.All(each => each);
                allResults.Add(boolResultsOk);
            }

            // Or Mode
            bool truly = allResults.Any(each => each);

#if SAINTSFIELD_DEBUG && SAINTSFIELD_DEBUG_READ_ONLY
            Debug.Log($"{property.name} final={truly}/ars={string.Join(",", allResults)}");
#endif
            return ("", truly);
        }

    }
}
