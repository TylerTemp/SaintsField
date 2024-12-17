#if UNITY_2021_3_OR_NEWER
using System;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.HandleDrawers.DrawLabel
{
    public partial class DrawLabelAttributeDrawer
    {
        #region UIToolkit
        private static string NameDrawLabel(SerializedProperty property) => $"{property.propertyPath}_DrawLabel";

        private LabelInfo _labelInfoUIToolkit;

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            DrawLabelAttribute drawLabelAttribute = (DrawLabelAttribute)saintsAttribute;
            Util.TargetWorldPosInfo targetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(drawLabelAttribute.Space, property, info, parent);
            if (targetWorldPosInfo.Error != "")
            {
                return new HelpBox(targetWorldPosInfo.Error, HelpBoxMessageType.Error);
            }

            _labelInfoUIToolkit = new LabelInfo
            {
                Content = drawLabelAttribute.Content,
                ActualContent = drawLabelAttribute.Content,
                IsCallback = drawLabelAttribute.IsCallback,
                EColor = drawLabelAttribute.EColor,
                TargetWorldPosInfo = targetWorldPosInfo,
            };

            return null;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            VisualElement child = new VisualElement
            {
                name = NameDrawLabel(property),
            };
            child.RegisterCallback<AttachToPanelEvent>(_ => SceneView.duringSceneGui += OnSceneGUIUIToolkit);
            child.RegisterCallback<DetachFromPanelEvent>(_ => SceneView.duringSceneGui -= OnSceneGUIUIToolkit);
            container.Add(child);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            if (_labelInfoUIToolkit.IsCallback)
            {
                object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

                (string error, object value) =
                    Util.GetOf<object>(_labelInfoUIToolkit.Content, null, property, info, parent);

                if (error != "")
                {
#if SAINTSFIELD_DEBUG
                    Debug.LogError(error);
#endif
                    return;
                }

                if (value is IWrapProp wrapProp)
                {
                    value = Util.GetWrapValue(wrapProp);
                }

                _labelInfoUIToolkit.ActualContent = $"{value}";
            }

            if (!_labelInfoUIToolkit.TargetWorldPosInfo.IsTransform)
            {
                DrawLabelAttribute drawLabelAttribute = (DrawLabelAttribute)saintsAttribute;
                object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;
                if(parent != null)
                {
                    _labelInfoUIToolkit.TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(drawLabelAttribute.Space, property, info, parent);
                }
            }
        }

        private GUIStyle _guiStyleUIToolkit;

        private void OnSceneGUIUIToolkit(SceneView sceneView)
        {
            OnSceneGUIInternal(sceneView, _labelInfoUIToolkit);
        }
        #endregion


    }
}
#endif
