using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Drawers.HandleDrawers.DrawLabel
{
    public partial class DrawLabelAttributeDrawer
    {
        #region IMGUI

        private readonly Dictionary<string, LabelInfo> IdToLabelInfo = new Dictionary<string, LabelInfo>();
        private static string GetKey(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";

        private string _cacheKey = "";

        protected override void ImGuiOnDispose()
        {
            base.ImGuiOnDispose();
            // ReSharper disable once InvertIf
            SceneView.duringSceneGui -= OnSceneGUIIMGUI;
            IdToLabelInfo.Remove(_cacheKey);

        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            return true;
        }

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label,
            float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            return 0;
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property,
            GUIContent label, ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            _cacheKey = GetKey(property);
            // ReSharper disable once InvertIf
            if (!IdToLabelInfo.TryGetValue(_cacheKey, out LabelInfo labelInfo))
            {
                DrawLabelAttribute drawLabelAttribute = (DrawLabelAttribute)saintsAttribute;

                Util.TargetWorldPosInfo targetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(drawLabelAttribute.Space, property, info, parent);
                if (targetWorldPosInfo.Error != "")
                {
                    Debug.LogError(targetWorldPosInfo.Error);
                    return position;
                }

                labelInfo = new LabelInfo
                {
                    Space = drawLabelAttribute.Space,
                    Content = drawLabelAttribute.Content,
                    ActualContent = drawLabelAttribute.Content,
                    IsCallback = drawLabelAttribute.IsCallback,
                    EColor = drawLabelAttribute.EColor,
                    TargetWorldPosInfo = targetWorldPosInfo,
                    GUIStyle = drawLabelAttribute.EColor == EColor.White
                        ? GUI.skin.label
                        : new GUIStyle
                        {
                            normal = { textColor = drawLabelAttribute.EColor.GetColor() },
                        },
                };
                IdToLabelInfo[_cacheKey] = labelInfo;
                ImGuiEnsureDispose(property.serializedObject.targetObject);
                SceneView.duringSceneGui += OnSceneGUIIMGUI;
                SceneView.RepaintAll();
                return position;
            }

            if (!labelInfo.TargetWorldPosInfo.IsTransform)
            {
                labelInfo.TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(labelInfo.Space, property, info, parent);
            }

            if (!labelInfo.IsCallback)
            {
                return position;
            }

            (string valueError, object value) = Util.GetOf<object>(labelInfo.Content, null, property, info, parent);
            if (valueError != "")
            {
                Debug.LogError(valueError);
                return position;
            }

            if (value is IWrapProp wrapProp)
            {
                value = Util.GetWrapValue(wrapProp);
            }

            labelInfo.ActualContent = $"{value}";
            return position;
        }

        private void OnSceneGUIIMGUI(SceneView sceneView)
        {
            if (IdToLabelInfo.TryGetValue(_cacheKey, out LabelInfo labelInfo))
            {
                OnSceneGUIInternal(sceneView, labelInfo);
            }
        }

        #endregion
    }
}
