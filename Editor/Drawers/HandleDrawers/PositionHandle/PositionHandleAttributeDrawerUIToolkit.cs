#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;

using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.HandleDrawers.PositionHandle
{
    public partial class PositionHandleAttributeDrawer
    {
        private static string NamePositionHandle(SerializedProperty property) => $"{property.propertyPath}_PositionHandle";

        private PositionHandleInfo _positionHandleInfoUIToolkit;

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            return null;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            PositionHandleAttribute positionHandleAttribute = (PositionHandleAttribute)saintsAttribute;
            _positionHandleInfoUIToolkit = new PositionHandleInfo
            {
                Error = "",

                SerializedProperty = property,
                MemberInfo = info,
                Parent = parent,
                Space = positionHandleAttribute.Space,

                TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfoSpace(positionHandleAttribute.Space, property, info, parent),
            };

            VisualElement child = new VisualElement
            {
                name = NamePositionHandle(property),
            };
            child.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                SceneView.duringSceneGui += OnSceneGUIUIToolkit;
                SceneView.RepaintAll();
            });
            child.RegisterCallback<DetachFromPanelEvent>(_ => SceneView.duringSceneGui -= OnSceneGUIUIToolkit);
            container.Add(child);
        }

        // private GUIStyle _guiStyleUIToolkit;

        private void OnSceneGUIUIToolkit(SceneView sceneView)
        {
            if (_positionHandleInfoUIToolkit.TargetWorldPosInfo.Error != "")
            {
                return;
            }

            try
            {
                string _ = _positionHandleInfoUIToolkit.SerializedProperty.propertyPath;
            }
            catch (NullReferenceException)
            {
                Debug.LogWarning("Property disposed, removing SceneGUI");
                SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
                return;
            }
            catch (ObjectDisposedException)
            {
                Debug.LogWarning("Property disposed, removing SceneGUI");
                SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
                return;
            }

            OnSceneGUIInternal(sceneView, _positionHandleInfoUIToolkit);
        }
    }
}

#endif
