using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.DrawLabel
{
    public partial class DrawLabelAttributeDrawer
    {
        private readonly Dictionary<string, LabelInfo> _idToLabelInfo = new Dictionary<string, LabelInfo>();

        private LabelInfo EnsureKey(DrawLabelAttribute drawLabelAttribute, SerializedProperty property, MemberInfo info,
            object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if(!_idToLabelInfo.TryGetValue(key, out LabelInfo labelInfo))
            {
                labelInfo = new LabelInfo
                {
                    DrawLabelAttribute = drawLabelAttribute,
                    SerializedProperty = property,
                    MemberInfo = info,
                    Parent = parent,
                    Error = "",

                    Content = drawLabelAttribute.Content,
                    Color = drawLabelAttribute.Color,
                };
                _idToLabelInfo[key] = labelInfo;

                // ReSharper disable once InconsistentNaming
                void OnSceneGUIIMGUI(SceneView sceneView)
                {
                    if (!_idToLabelInfo.TryGetValue(key, out LabelInfo innerLabelInfo))
                    {
                        return;
                    }
                    OnSceneGUIInternal(sceneView, innerLabelInfo);
                }

                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    _idToLabelInfo.Remove(key);
                    SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                });
                SceneView.duringSceneGui += OnSceneGUIIMGUI;
                SceneView.RepaintAll();
            }

            labelInfo.SerializedProperty = property;
            labelInfo.MemberInfo = info;
            labelInfo.Parent = parent;
            return labelInfo;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return EnsureKey((DrawLabelAttribute) saintsAttribute, property, info, parent).Error != "";
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string error = EnsureKey((DrawLabelAttribute)saintsAttribute, property, info, parent).Error;
            return error == ""? 0: ImGuiHelpBox.GetHeight(error, width, MessageType.Error);
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string error = EnsureKey((DrawLabelAttribute)saintsAttribute, property, info, parent).Error;
            return error == ""? position: ImGuiHelpBox.Draw(position, error, MessageType.Error);
        }

    }
}
