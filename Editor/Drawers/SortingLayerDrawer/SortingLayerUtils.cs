using System;
using System.Reflection;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
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

        private enum SortingLayerItemType
        {
            None,
            Normal,
            EmptyString,
            OpenEditor,
        }

        private readonly struct SortingLayerInfo: IEquatable<SortingLayerInfo>
        {
            public readonly string Name;
            public readonly int Id;
            public readonly SortingLayerItemType Type;

            public SortingLayerInfo(int id, string name)
            {
                Name = name;
                Id = id;
                Type = SortingLayerItemType.Normal;
            }

            public SortingLayerInfo(SortingLayerItemType type, string name)
            {
                Name = name;
                Id = int.MinValue;
                Type = type;
            }

            public bool Equals(SortingLayerInfo other)
            {
                return Id == other.Id && Type == other.Type;
            }

            public override bool Equals(object obj)
            {
                return obj is SortingLayerInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Id, (int)Type);
            }
        }

        public static void MakeDropdown(bool isString, object curValue, VisualElement root, Action<object> onValueChangedCallback)
        {
            AdvancedDropdownList<SortingLayerInfo> dropdown = new AdvancedDropdownList<SortingLayerInfo>();
            if (isString)
            {
                dropdown.Add("[Empty String]", new SortingLayerInfo(SortingLayerItemType.EmptyString, ""));
            }

            // string selectedName = null;
            // int selectedIndex = -1;
            SortingLayerInfo selectedSortingLayer = default;
            if(SortingLayer.layers.Length > 0)
            {
                dropdown.AddSeparator();

                foreach (SortingLayer sortingLayer in SortingLayer.layers)
                {
                    SortingLayerInfo layerInfo = new SortingLayerInfo(sortingLayer.id, sortingLayer.name);
                    dropdown.Add(new AdvancedDropdownList<SortingLayerInfo>(
                        $"{sortingLayer.name} <color=#808080>{sortingLayer.id}</color>",
                        layerInfo));
                    // ReSharper disable once InvertIf
                    if (isString && sortingLayer.name == (string)curValue
                        || !isString && sortingLayer.id == (int)curValue)
                    {
                        selectedSortingLayer = layerInfo;
                    }
                }
            }

            if(SortingLayer.layers.Length > 0)
            {
                dropdown.AddSeparator();
            }

            dropdown.Add("Edit Sorting Layers", new SortingLayerInfo(SortingLayerItemType.OpenEditor, null), false, "d_editicon.sml");

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = new []{(object)selectedSortingLayer},
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
                    SortingLayerInfo curInfo = (SortingLayerInfo)curItem;
                    switch (curInfo.Type)
                    {
                        case SortingLayerItemType.Normal:
                        {
                            if (isString)
                            {
                                onValueChangedCallback.Invoke(curInfo.Name);
                            }
                            else
                            {
                                onValueChangedCallback.Invoke(curInfo.Id);
                            }
                        }
                            break;
                        case SortingLayerItemType.EmptyString:
                        {
                            Debug.Assert(isString);
                            onValueChangedCallback.Invoke("");
                        }
                            break;
                        case SortingLayerItemType.OpenEditor:
                        {
                            OpenSortingLayerInspector();
                        }
                            break;
                        case SortingLayerItemType.None:
                        default:
                            throw new ArgumentOutOfRangeException(nameof(curInfo), curInfo.Type, null);
                    }
                }
            );

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }
    }
}
