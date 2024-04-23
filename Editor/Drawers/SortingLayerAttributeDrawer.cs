using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_3_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(SortingLayerAttribute))]
    public class SortingLayerAttributeDrawer: SaintsPropertyDrawer
    {
        private static string[] GetLayers()
        {
            // Type internalEditorUtilityType = typeof(UnityEditorInternal.InternalEditorUtility);
            // PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
            // Debug.Assert(sortingLayersProperty != null);
            // return (string[])sortingLayersProperty.GetValue(null, Array.Empty<object>());
            return SortingLayer.layers.Select(each => each.name).ToArray();
        }

        private static void OpenSortingLayerInspector()
        {
            // TagManagerInspector.ShowWithInitialExpansion(TagManagerInspector.InitialExpansionState.Layers)
            Type tagManagerInspectorType = Type.GetType("UnityEditor.TagManagerInspector, UnityEditor");
            // Get the method Info for the ShowWithInitialExpansion method
            MethodInfo showWithInitialExpansionMethod = tagManagerInspectorType.GetMethod("ShowWithInitialExpansion", BindingFlags.Static | BindingFlags.NonPublic);
            Type initialExpansionStateType = tagManagerInspectorType.GetNestedType("InitialExpansionState", BindingFlags.NonPublic);
            object layersEnumValue = Enum.Parse(initialExpansionStateType, "SortingLayers");
            // Invoke the ShowWithInitialExpansion method with the Layers enum value
            showWithInitialExpansionMethod.Invoke(null, new object[] { layersEnumValue });
        }

        #region IMGUI

        protected override float GetFieldHeight(SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, bool hasLabelWidth, object parent)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        protected override void DrawField(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            string[] layers = GetLayers();

            int selectedIndex = property.propertyType == SerializedPropertyType.Integer ? property.intValue : Array.IndexOf(layers, property.stringValue);

            // ReSharper disable once ConvertToUsingDeclaration
            using (EditorGUI.ChangeCheckScope changed = new EditorGUI.ChangeCheckScope())
            {
                int newIndex = EditorGUI.Popup(position, label, selectedIndex,
                    layers
                        .Select(each => each == ""? new GUIContent("<empty string>") : new GUIContent(each))
                        .Concat(new[]{GUIContent.none, new GUIContent("Edit Sorting Layers...")})
                        .ToArray());
                // ReSharper disable once InvertIf
                if (changed.changed)
                {
                    if (newIndex >= layers.Length)
                    {
                        OpenSortingLayerInspector();
                        return;
                    }

                    if (property.propertyType == SerializedPropertyType.Integer)
                    {
                        property.intValue = newIndex;
                    }
                    else
                    {
                        property.stringValue = layers[newIndex];
                    }
                }
            }
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            FieldInfo info,
            object parent) => property.propertyType != SerializedPropertyType.Integer && property.propertyType != SerializedPropertyType.String;

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => property.propertyType != SerializedPropertyType.Integer && property.propertyType != SerializedPropertyType.String
            ? ImGuiHelpBox.GetHeight($"Expect string or int, get {property.propertyType}", width, MessageType.Error)
            : 0f;

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, FieldInfo info, object parent) => ImGuiHelpBox.Draw(position, $"Expect string or int, get {property.propertyType}", MessageType.Error);
        #endregion

#if UNITY_2021_3_OR_NEWER

        #region UIToolkit

        private static string NameButtonField(SerializedProperty property) => $"{property.propertyPath}__SortingLayer_Button";
        private static string NameButtonLabelField(SerializedProperty property) => $"{property.propertyPath}__SortingLayer_ButtonLabel";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__SortingLayer_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            VisualElement container, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonUIToolkit dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit();
            dropdownButton.Button.style.flexGrow = 1;
            dropdownButton.Button.name = NameButtonField(property);
            dropdownButton.Label.name = NameButtonLabelField(property);

            VisualElement root = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };

            Label label = Util.PrefixLabelUIToolKit(property.displayName, 0);
            label.AddToClassList("unity-label");
            root.Add(label);
            root.Add(dropdownButton.Button);

            root.AddToClassList(ClassAllowDisable);

            return root;
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            HelpBox helpBox = new HelpBox("", HelpBoxMessageType.Error)
            {
                style =
                {
                    display = DisplayStyle.None,
                },
                name = NameHelpBox(property),
            };
            helpBox.AddToClassList(ClassAllowDisable);
            return helpBox;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index, VisualElement container,
            Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            Label buttonLabel = container.Q<Label>(NameButtonLabelField(property));
            (string[] _, int _, string displayName) = GetSelected(property);
            buttonLabel.text = displayName;

            container.Q<Button>(NameButtonField(property)).clicked += () =>
                ShowDropdown(property, container, parent, onValueChangedCallback);
        }

        private static void ShowDropdown(SerializedProperty property,
            VisualElement container, object parent, Action<object> onChange)
        {
            (string[] layers, int selectedIndex, string _) = GetSelected(property);

            GenericDropdownMenu genericDropdownMenu = new GenericDropdownMenu();
            Button button = container.Q<Button>(NameButtonField(property));

            // if (layers.Length == 0)
            // {
            //     genericDropdownMenu.AddDisabledItem("No layers", false);
            //     genericDropdownMenu.DropDown(button.worldBound, button, true);
            //     return;
            // }

            Label buttonLabel = container.Q<Label>(NameButtonLabelField(property));

            foreach (int index in Enumerable.Range(0, layers.Length))
            {
                int curIndex = index;
                string curItem = layers[index];

                string curName = $"{index}: {curItem}";

                genericDropdownMenu.AddItem(curName, index == selectedIndex, () =>
                {

                    if(property.propertyType == SerializedPropertyType.String)
                    {
                        property.stringValue = curItem;
                        property.serializedObject.ApplyModifiedProperties();
                        onChange.Invoke(curItem);
                    }
                    else
                    {
                        property.intValue = curIndex;
                        property.serializedObject.ApplyModifiedProperties();
                        onChange.Invoke(curIndex);
                    }
                    buttonLabel.text = curName;
                });
            }

            if (layers.Length > 0)
            {
                genericDropdownMenu.AddSeparator("");
            }
            genericDropdownMenu.AddItem("Edit Sorting Layers...", false, OpenSortingLayerInspector);

            genericDropdownMenu.DropDown(button.worldBound, button, true);
        }

        private static (string[] layers, int index, string display) GetSelected(SerializedProperty property)
        {
            string[] layers = GetLayers();
            if(property.propertyType == SerializedPropertyType.String)
            {
                string value = property.stringValue;
                int index = Array.IndexOf(layers, value);
                return (layers, index, index == -1? $"?: {value}": $"{index}: {value}");
            }
            else
            {
                int index = property.intValue;
                if(index < 0 || index >= layers.Length)
                {
                    return (layers, -1, $"{index}: ?");
                }
                return (layers, index, $"{index}: {layers[index]}");
            }
        }

        #endregion

#endif
    }
}
