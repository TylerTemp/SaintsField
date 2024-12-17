#if SAINTSFIELD_SAINTSDRAW || SAINTSDRAW && !SAINTSFIELD_SAINTSDRAW_DISABLE
#if UNITY_2021_3_OR_NEWER

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Drawers.HandleDrawers.SaintsArrow
{
    public  partial class SaintsArrowAttributeDrawer
    {

        #region UIToolkit
        private static string NameSaintsArrow(SerializedProperty property) => $"{property.propertyPath}_SaintsArrow";

        private ArrowInfo _arrowInfoUIToolkit;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            ArrowConstInfo arrowConstInfo = new ArrowConstInfo
            {
                SaintsArrowAttribute = (SaintsArrowAttribute) saintsAttribute,
                Property = property,
                Info = info,
                Parent = parent,
            };

            _arrowInfoUIToolkit = GetArrowInfo(arrowConstInfo);

            VisualElement child = new VisualElement
            {
                name = NameSaintsArrow(property),
            };
            child.RegisterCallback<AttachToPanelEvent>(_ => SceneView.duringSceneGui += OnSceneGUIUIToolkit);
            child.RegisterCallback<DetachFromPanelEvent>(_ => SceneView.duringSceneGui -= OnSceneGUIUIToolkit);
            container.Add(child);
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            // ReSharper disable once MergeIntoNegatedPattern
            if (_arrowInfoUIToolkit == null || _arrowInfoUIToolkit.Error != "")
            {
                return;
            }

            if (!_arrowInfoUIToolkit.StartTargetWorldPosInfo.IsTransform || !_arrowInfoUIToolkit.EndTargetWorldPosInfo.IsTransform)
            {
                _arrowInfoUIToolkit = GetArrowInfo(_arrowInfoUIToolkit.ArrowConstInfo);
            }
        }

        private GUIStyle _guiStyleUIToolkit;

        private void OnSceneGUIUIToolkit(SceneView sceneView)
        {
            if (_arrowInfoUIToolkit is null)
            {
                // Debug.Log($"no config");
                return;
            }

            // Debug.Log($"render {_arrowInfoUIToolkit}");

            // ReSharper disable once InvertIf
            if (!OnSceneGUIInternal(sceneView, _arrowInfoUIToolkit))
            {
                Debug.LogWarning($"Target disposed, remove SceneGUI");
                SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
            }
        }

        #endregion

    }
}

#endif
#endif
