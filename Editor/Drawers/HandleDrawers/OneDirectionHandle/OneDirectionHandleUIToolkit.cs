#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.HandleDrawers.OneDirectionHandle
{
    public abstract partial class OneDirectionHandleBase
    {
        private static string NameSaintsArrow(SerializedProperty property) => $"{property.propertyPath}_OneDirectionHandle";

        private OneDirectionInfo _oneDirectionInfoUIToolkit;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            OneDirectionBaseAttribute oneDirectionBaseAttribute = (OneDirectionBaseAttribute)saintsAttribute;
            _oneDirectionInfoUIToolkit = new OneDirectionInfo
            {
                Error = "",

                OneDirectionAttribute = oneDirectionBaseAttribute,
                SerializedProperty = property,
                MemberInfo = info,
                Parent = parent,

                Color = oneDirectionBaseAttribute.Color,
            };

            VisualElement child = new VisualElement
            {
                name = NameSaintsArrow(property),
            };
            child.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                SceneView.duringSceneGui += OnSceneGUIUIToolkit;
                SceneView.RepaintAll();
            });
            child.RegisterCallback<DetachFromPanelEvent>(_ => SceneView.duringSceneGui -= OnSceneGUIUIToolkit);
            container.Add(child);
        }

        private void OnSceneGUIUIToolkit(SceneView sceneView)
        {
            // Debug.Log($"render {_arrowInfoUIToolkit}");

            // ReSharper disable once InvertIf
            if (!OnSceneGUIInternal(sceneView, _oneDirectionInfoUIToolkit))
            {
                Debug.LogWarning($"Target disposed, remove SceneGUI");
                SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
            }
        }
    }
}

#endif
