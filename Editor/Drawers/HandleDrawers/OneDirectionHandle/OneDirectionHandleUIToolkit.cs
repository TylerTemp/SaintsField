using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_2021_3_OR_NEWER

namespace SaintsField.Editor.Drawers.HandleDrawers.OneDirectionHandle
{
    public abstract partial class OneDirectionHandleBase
    {

        #region UIToolkit
        private static string NameSaintsArrow(SerializedProperty property) => $"{property.propertyPath}_OneDirectionHandle";

        private OneDirectionInfo _oneDirectionInfoUIToolkit;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            OneDirectionConstInfo oneDirectionConstInfo = new OneDirectionConstInfo
            {
                OneDirectionAttribute = (SaintsArrowAttribute) saintsAttribute,
                Property = property,
                Info = info,
                Parent = parent,
            };

            _oneDirectionInfoUIToolkit = GetArrowInfo(oneDirectionConstInfo);

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
            if (_oneDirectionInfoUIToolkit == null || _oneDirectionInfoUIToolkit.Error != "")
            {
                return;
            }

            if (!_oneDirectionInfoUIToolkit.StartTargetWorldPosInfo.IsTransform || !_oneDirectionInfoUIToolkit.EndTargetWorldPosInfo.IsTransform)
            {
                _oneDirectionInfoUIToolkit = GetArrowInfo(_oneDirectionInfoUIToolkit.OneDirectionConstInfo);
            }
        }

        private GUIStyle _guiStyleUIToolkit;

        protected void OnSceneGUIUIToolkit(SceneView sceneView)
        {
            if (_oneDirectionInfoUIToolkit is null)
            {
                // Debug.Log($"no config");
                return;
            }

            // Debug.Log($"render {_arrowInfoUIToolkit}");

            // ReSharper disable once InvertIf
            if (!OnSceneGUIInternal(sceneView, _oneDirectionInfoUIToolkit))
            {
                Debug.LogWarning($"Target disposed, remove SceneGUI");
                SceneView.duringSceneGui -= OnSceneGUIUIToolkit;
            }
        }

        #endregion

    }
}

#endif
