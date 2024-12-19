using System;
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

        private readonly Dictionary<string, LabelInfo> _idToLabelInfo = new Dictionary<string, LabelInfo>();
        private static string GetKey(SerializedProperty property) => $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}";

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
            string key = GetKey(property);

            if (_idToLabelInfo.TryGetValue(key, out LabelInfo labelInfo))
            {
                return position;
            }

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
            _idToLabelInfo[key] = labelInfo;
            SceneView.duringSceneGui += OnSceneGUIIMGUI;
            SceneView.RepaintAll();

            Selection.selectionChanged += OnSelectionChanged;

            return position;

            void OnSelectionChanged()
            {
                bool remove = false;
                UnityEngine.Object oriObject = null;
                try
                {
                    oriObject = property.serializedObject.targetObject;
                }
                catch (Exception)
                {
                    remove = true;
                }

                if (!remove)
                {
                    if (oriObject == null)
                    {
                        remove = true;
                    }
                    else
                    {
                        remove = Array.IndexOf(Selection.objects, oriObject) == -1;
                    }
                }

                if (remove)
                {
                    Unsub();
                }
            }

            // ReSharper disable once InconsistentNaming
            void OnSceneGUIIMGUI(SceneView sceneView)
            {
                if (!_idToLabelInfo.TryGetValue(key, out LabelInfo cachedLabelInfo))
                {
                    return;
                }

                if (!cachedLabelInfo.TargetWorldPosInfo.IsTransform)
                {
                    cachedLabelInfo.TargetWorldPosInfo = Util.GetPropertyTargetWorldPosInfo(cachedLabelInfo.Space, property, info, parent);
                }

                if (cachedLabelInfo.IsCallback)
                {
                    (string valueError, object value) =
                        Util.GetOf<object>(cachedLabelInfo.Content, null, property, info, parent);
                    if (valueError != "")
                    {
#if SAINTSFIELD_DEBUG
                        Debug.LogError(valueError);
#endif
                        Unsub();
                        return;
                    }

                    if (value is IWrapProp wrapProp)
                    {
                        value = Util.GetWrapValue(wrapProp);
                    }
                    labelInfo.ActualContent = $"{value}";
                }

                OnSceneGUIInternal(sceneView, cachedLabelInfo);
            }

            void Unsub()
            {
                SceneView.duringSceneGui -= OnSceneGUIIMGUI;
                _idToLabelInfo.Remove(key);
            }
        }

        #endregion
    }
}
