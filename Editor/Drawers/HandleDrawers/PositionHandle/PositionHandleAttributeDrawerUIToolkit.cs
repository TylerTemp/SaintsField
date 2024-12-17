#if UNITY_2021_3_OR_NEWER
using System;
using System.Reflection;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;

using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.HandleDrawers.PositionHandle
{
    public partial class PositionHandleAttributeDrawer
    {
        #region UIToolkit
        private static string NamePositionHandle(SerializedProperty property) => $"{property.propertyPath}_PositionHandle";

        private PositionHandleInfo _positionHandleInfoUIToolkit;

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index, VisualElement container, FieldInfo info, object parent)
        {
            return null;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            PositionHandleAttribute positionHandleAttribute = (PositionHandleAttribute)saintsAttribute;
            _positionHandleInfoUIToolkit = new PositionHandleInfo
            {
                Property = property,
                Info = info,
                Parent = parent,
                Space = positionHandleAttribute.Space,

                TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(positionHandleAttribute.Space, property, info, parent),
            };

            VisualElement child = new VisualElement
            {
                name = NamePositionHandle(property),
            };
            child.RegisterCallback<AttachToPanelEvent>(_ => SceneView.duringSceneGui += OnSceneGUIUIToolkit);
            child.RegisterCallback<DetachFromPanelEvent>(_ => SceneView.duringSceneGui -= OnSceneGUIUIToolkit);
            container.Add(child);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            if (_positionHandleInfoUIToolkit.TargetWorldPosInfo.Error != "")
            {
                return;
            }

            if (_positionHandleInfoUIToolkit.TargetWorldPosInfo.IsTransform)
            {
                return;
            }

            object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

            if (parent == null)
            {
                return;
            }

            _positionHandleInfoUIToolkit.TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(_positionHandleInfoUIToolkit.Space, property, info, parent);
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
                string _ = _positionHandleInfoUIToolkit.Property.propertyPath;
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

            if(!OnSceneGUIInternal(sceneView, _positionHandleInfoUIToolkit)) {
                Debug.LogWarning("Target disposed, removing SceneGUI");
                SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
            }
        }
        #endregion

    }
}

#endif
