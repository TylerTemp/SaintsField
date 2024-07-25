#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using UnityEditor;
using SaintsField.AiNavigation;
using SaintsField.Editor.Utils;
using UnityEditor.UIElements;
using UnityEngine;

#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif  // UNITY_2021_3_OR_NEWER

#endif  // SAINTSFIELD_AI_NAVIGATION

namespace SaintsField.Editor.Drawers.AiNavigation
{
#if SAINTSFIELD_AI_NAVIGATION && !SAINTSFIELD_AI_NAVIGATION_DISABLED
    [CustomPropertyDrawer(typeof(NavMeshAreaMaskAttribute))]
    public class NavMeshAreaMaskAttributeDrawer: SaintsPropertyDrawer
    {

        #region IMGUI

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute,
            OnGUIPayload onGUIPayload,
            FieldInfo info, object parent)
        {
            string[] options = AiNavigationUtils.GetNavMeshAreas().Select(each => $"{each.Mask}: {each.Name}").ToArray();

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newMask = EditorGUI.MaskField(position, label, property.intValue, options);
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    property.intValue = newMask;
                }
            }
        }

        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UI Toolkit

        private static string NameMaskField(SerializedProperty property) => $"{property.propertyPath}__NavMeshAreaMask";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, FieldInfo info, object parent)
        {
            MaskField maskField = new MaskField(property.displayName)
            {
                userData = new List<AiNavigationUtils.NavMeshArea>(),
                name = NameMaskField(property),
                style =
                {
                    flexGrow = 1,
                },
            };
            maskField.BindProperty(property);
            maskField.AddToClassList("unity-base-field__aligned");
            maskField.AddToClassList(ClassAllowDisable);
            return maskField;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            container.Q<MaskField>(NameMaskField(property)).RegisterValueChangedCallback(evt =>
            {
                int value = evt.newValue;
                onValueChangedCallback.Invoke(value);
            });
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            MaskField maskField = container.Q<MaskField>(NameMaskField(property));
            List<AiNavigationUtils.NavMeshArea> oldData = (List<AiNavigationUtils.NavMeshArea>)maskField.userData;

            List<AiNavigationUtils.NavMeshArea> newData = AiNavigationUtils.GetNavMeshAreas().ToList();
            if(newData.SequenceEqual(oldData) && maskField.value == property.intValue)
            {
                return;
            }

            // Debug.Log("Reset");

            maskField.userData = newData;
            maskField.choices = newData.Select(each => each.Name).ToList();
            // maskField.formatSelectedValueCallback = listItem => $"[{listItem}]";
            maskField.formatSelectedValueCallback =  maskField.formatListItemCallback = selected =>
            {
                int i = newData.FindIndex(each => each.Name == selected);
                return $"{newData[i].Mask}: {newData[i].Name}";
            };
            // maskField.value = property.intValue;
            maskField.SetValueWithoutNotify(property.intValue);
        }

        #endregion

#endif
    }
#endif  // SAINTSFIELD_AI_NAVIGATION
}
