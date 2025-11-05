using System;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.SceneDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.SortingLayerDrawer
{
    public static class SortingLayerUtils
    {
        public static void OpenSortingLayerInspector()
        {
            // TagManagerInspector.ShowWithInitialExpansion(TagManagerInspector.InitialExpansionState.Layers)
            Type tagManagerInspectorType = Type.GetType("UnityEditor.TagManagerInspector, UnityEditor");
            // Get the method Info for the ShowWithInitialExpansion method
            if (tagManagerInspectorType == null)
            {
                return;
            }

            MethodInfo showWithInitialExpansionMethod = tagManagerInspectorType.GetMethod("ShowWithInitialExpansion", BindingFlags.Static | BindingFlags.NonPublic);
            if (showWithInitialExpansionMethod == null)
            {
                return;
            }

            Type initialExpansionStateType = tagManagerInspectorType.GetNestedType("InitialExpansionState", BindingFlags.NonPublic);
            object layersEnumValue = Enum.Parse(initialExpansionStateType, "SortingLayers");
            // Invoke the ShowWithInitialExpansion method with the Layers enum value
            showWithInitialExpansionMethod.Invoke(null, new object[] { layersEnumValue });
        }

        public static void MakeDropdown(bool isString, object curValue, VisualElement root, Action<object> onValueChangedCallback)
        {
            AdvancedDropdownList<(string path, int index)> dropdown = new AdvancedDropdownList<(string path, int index)>();
            if (isString)
            {
                dropdown.Add("[Empty String]", (string.Empty, -1));
                dropdown.AddSeparator();
            }

            string selectedName = null;
            int selectedIndex = -1;
            foreach (SortingLayer sortingLayer in SortingLayer.layers)
            {
                // dropdown.Add(path, (path, index));
                dropdown.Add(new AdvancedDropdownList<(string path, int index)>($"<color=#808080>{sortingLayer.value}</color> {sortingLayer.name}", (sortingLayer.name, sortingLayer.value)));
                // ReSharper disable once InvertIf
                if (isString && sortingLayer.name == (string)curValue
                    || !isString && sortingLayer.value == (int)curValue)
                {
                    selectedName = sortingLayer.name;
                    selectedIndex = sortingLayer.value;
                }
            }

            dropdown.AddSeparator();
            dropdown.Add("Edit Sorting Layers", ("", -2), false, "d_editicon.sml");

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selectedIndex >= 0 ? new object[] { (selectedName, selectedIndex) } : Array.Empty<object>(),
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            (Rect worldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsAdvancedDropdownUIToolkit sa = new SaintsAdvancedDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                false,
                (_, curItem) =>
                {
                    (string path, int index) = ((string path, int index))curItem;
                    switch (index)
                    {
                        case -1:
                        {
                            Debug.Assert(isString);
                            onValueChangedCallback.Invoke("");
                        }
                            break;
                        case -2:
                        {
                            OpenSortingLayerInspector();
                        }
                            break;
                        default:
                        {
                            if (isString)
                            {
                                onValueChangedCallback.Invoke(path);
                            }
                            else
                            {
                                onValueChangedCallback.Invoke(index);
                            }
                        }
                            break;
                    }
                }
            );

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }
    }
}
