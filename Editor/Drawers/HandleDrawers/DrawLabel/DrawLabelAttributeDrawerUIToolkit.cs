#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.HandleDrawers.DrawLabel
{
    public partial class DrawLabelAttributeDrawer
    {
        private static string NameDrawLabel(SerializedProperty property) => $"{property.propertyPath}_DrawLabel";

        // private LabelInfo _labelInfoUIToolkit;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            DrawLabelAttribute drawLabelAttribute = (DrawLabelAttribute)saintsAttribute;
            Util.TargetWorldPosInfo targetWorldPosInfo = Util.GetPropertyTargetWorldPosInfoSpace(drawLabelAttribute.Space, property, info, parent);
            if (targetWorldPosInfo.Error != "")
            {
                HelpBox helpBox = new HelpBox(targetWorldPosInfo.Error, HelpBoxMessageType.Error);
                container.Add(helpBox);
                return;
            }

            LabelInfo labelInfoUIToolkit = new LabelInfo
            {
                DrawLabelAttribute = drawLabelAttribute,
                SerializedProperty = property,
                MemberInfo = info,
                Parent = parent,
                Error = "",

                Content = drawLabelAttribute.Content,
                Color = drawLabelAttribute.Color,
            };

            VisualElement child = new VisualElement
            {
                name = NameDrawLabel(property),
            };

            child.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                SceneView.duringSceneGui += OnSceneGUIUIToolkit;
                SceneView.RepaintAll();
            });
            child.RegisterCallback<DetachFromPanelEvent>(_ => SceneView.duringSceneGui -= OnSceneGUIUIToolkit);
            container.Add(child);

            // container.schedule.Execute(() =>
            // {
            //     if (!SerializedUtils.IsOk(labelInfoUIToolkit.SerializedProperty))
            //     {
            //         SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
            //     }
            // }).Every(1000);
            return;

            // ReSharper disable once InconsistentNaming
            void OnSceneGUIUIToolkit(SceneView sceneView)
            {
                OnSceneGUIInternal(sceneView, labelInfoUIToolkit);
            }
        }
    }
}
#endif
