using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.ColorPalette;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_2021_3_OR_NEWER
namespace SaintsField.Editor.Drawers.ColorPaletteDrawer
{
    public partial class ColorPaletteAttributeDrawer
    {
        private static string ToggleButtonName(SerializedProperty property) => $"{property.propertyPath}__ColorPalette_ToggleButton";
        private static string DropdownName(SerializedProperty property) => $"{property.propertyPath}__ColorPalette_Dropdown";
        private static string SearchInputName(SerializedProperty property) => $"{property.propertyPath}__ColorPalette_SearchInput";
        private static string ColorButtonsName(SerializedProperty property) => $"{property.propertyPath}__ColorPalette_ColorButtons";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            return new ToolbarToggle
            {
                style =
                {
                    backgroundImage = Util.LoadResource<Texture2D>("pencil.png"),
#if UNITY_2022_2_OR_NEWER
                    backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Center),
                    backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat),
                    backgroundSize  = new BackgroundSize(14, 14),
#else
                    unityBackgroundScaleMode = ScaleMode.ScaleToFit,
#endif
                    paddingLeft = 8,
                    paddingRight = 8,
                },
                name = ToggleButtonName(property),
            };
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            VisualElement root = new VisualElement
            {
                style =
                {
                    // display = DisplayStyle.None,
                    backgroundColor = EColor.EditorEmphasized.GetColor(),
                    paddingTop = 2,
                    paddingBottom = 2,
                    paddingLeft = 2,
                    paddingRight = 4,
                },
            };

            VisualElement paletteInput = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                },
            };
            root.Add(paletteInput);

            UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit("Palette");
            dropdownButton.name = DropdownName(property);
            // dropdownButton.userData = new List<SaintsField.ColorPalette>();
            dropdownButton.userData = new PaletteSelectorInfo
            {
                SelectedPalettes = Array.Empty<SaintsField.ColorPalette>(),
                AllPalettes = new List<SaintsField.ColorPalette>(),
            };
            paletteInput.Add(dropdownButton);

            TextField searchField = new TextField
            {
                style =
                {
                    width = Length.Percent(20),
                },
                name = SearchInputName(property),
            };
            paletteInput.Add(searchField);

            VisualElement colors = new VisualElement
            {
                style =
                {
                    // display = DisplayStyle.None,
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                    justifyContent = Justify.FlexEnd,
                },
                name = ColorButtonsName(property),
            };
            root.Add(colors);

            return root;
        }

        // private readonly List<SaintsField.ColorPalette> _colorPalettes = new List<SaintsField.ColorPalette>();
        private class PaletteSelectorInfo
        {
            public IReadOnlyList<SaintsField.ColorPalette> SelectedPalettes;
            public List<SaintsField.ColorPalette> AllPalettes;
        }

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = container.Q<UIToolkitUtils.DropdownButtonField>(name: DropdownName(property));
            PaletteSelectorInfo colorPaletteInfo = (PaletteSelectorInfo) dropdownButton.userData;
            ColorPaletteAttribute colorPaletteAttribute = (ColorPaletteAttribute) saintsAttribute;
            FillColorPalettes(colorPaletteInfo.AllPalettes, colorPaletteAttribute.ColorPaletteSources, property, info, parent);

            // find the init color palette
            Color color = property.colorValue;
            SaintsField.ColorPalette initColorPalette =
                colorPaletteInfo.AllPalettes.FirstOrDefault(colorPalette =>
                    colorPalette.colors.Any(each => each.color == color)
                );

            if (initColorPalette != null)
            {
                colorPaletteInfo.SelectedPalettes = new[] { initColorPalette };
            }

            dropdownButton.ButtonElement.clicked += () =>
            {
                AdvancedDropdownMetaInfo dropdownMetaInfo = GetMetaInfo(colorPaletteInfo.SelectedPalettes, colorPaletteInfo.AllPalettes);

                float maxHeight = Screen.currentResolution.height - dropdownButton.worldBound.y - dropdownButton.worldBound.height - 100;
                Rect worldBound = dropdownButton.worldBound;
                if (maxHeight < 100)
                {
                    worldBound.y -= 100 + worldBound.height;
                    maxHeight = 100;
                }

                UnityEditor.PopupWindow.Show(worldBound, new SaintsAdvancedDropdownUIToolkit(
                    dropdownMetaInfo,
                    dropdownButton.worldBound.width,
                    maxHeight,
                    false,
                    (_, curItem) =>
                    {
                        if (curItem is null)
                        {
                            // Debug.Log("Open Editor");
                            ColorPaletteMenu.OpenWindow();
                            return;
                        }

                        colorPaletteInfo.SelectedPalettes = (IReadOnlyList<SaintsField.ColorPalette>) curItem;
                        RefreshColorButtons(container, property, onValueChangedCallback);
                    }
                ));
            };

            TextField searchInput = container.Q<TextField>(name: SearchInputName(property));
            searchInput.RegisterValueChangedCallback(_ =>
                RefreshColorButtons(container, property, onValueChangedCallback));

            RefreshColorButtons(container, property, onValueChangedCallback);
        }

        // protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
        //     VisualElement container, Action<object> onValueChanged, FieldInfo info)
        // {
        //     RefreshColorButtons(container, property, onValueChanged);
        // }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            RefreshColorButtons(container, property, onValueChangedCallback);
        }

        private static bool FillColorPalettes(List<SaintsField.ColorPalette> colorPalettes, IReadOnlyList<ColorPaletteAttribute.ColorPaletteSource> colorPaletteSources, SerializedProperty property, MemberInfo info, object parent)
        {
            List<SaintsField.ColorPalette> foundColorPalettes = new List<SaintsField.ColorPalette>();
            if (colorPaletteSources.Count == 0)
            {
                foundColorPalettes.AddRange(ColorPaletteRegister.ColorPalettes);
            }
            else
            {
                foreach (ColorPaletteAttribute.ColorPaletteSource colorPaletteSource in colorPaletteSources)
                {
                    SaintsField.ColorPalette findTarget = null;
                    if (colorPaletteSource.IsCallback)
                    {
                        string callback = colorPaletteSource.Name;
                        (string error, SaintsField.ColorPalette result) =
                            Util.GetOf<SaintsField.ColorPalette>(callback, null, property, info, parent);
                        if (error != "")
                        {
#if SAINTSFIELD_DEBUG
                            Debug.LogError(error);
#endif
                        }
                        else if (result != null)
                        {
                            findTarget = result;
                        }
                    }
                    else
                    {
                        findTarget =
                            ColorPaletteRegister.ColorPalettes.FirstOrDefault(each =>
                                each.displayName == colorPaletteSource.Name);
                    }

                    if (findTarget != null && !foundColorPalettes.Contains(findTarget))
                    {
                        foundColorPalettes.Add(findTarget);
                    }
                }
            }

            bool changed = false;

            // check add
            foreach (SaintsField.ColorPalette found in foundColorPalettes.Where(found => !colorPalettes.Contains(found)))
            {
                changed = true;
                colorPalettes.Add(found);
            }
            // check delete
            foreach (SaintsField.ColorPalette exist in colorPalettes.ToArray())
            {
                // ReSharper disable once InvertIf
                if (!foundColorPalettes.Contains(exist))
                {
                    changed = true;
                    colorPalettes.Remove(exist);
                }
            }

            return changed;
        }

        private static void RefreshColorButtons(VisualElement container, SerializedProperty property, Action<object> onValueChanged)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = container.Q<UIToolkitUtils.DropdownButtonField>(name: DropdownName(property));
            PaletteSelectorInfo paletteSelectorInfo = (PaletteSelectorInfo) dropdownButton.userData;

            dropdownButton.ButtonLabelElement.text = string.Join(",", paletteSelectorInfo.SelectedPalettes.Select(each => each.displayName));

            TextField searchInput = container.Q<TextField>(name: SearchInputName(property));
            string searchContent = searchInput.value.Trim();

            VisualElement colorsContainer = container.Q<VisualElement>(name: ColorButtonsName(property));
            colorsContainer.Clear();

            Color selectedColor = property.colorValue;

            foreach (SaintsField.ColorPalette.ColorEntry colorEntry in paletteSelectorInfo.SelectedPalettes.SelectMany(each => each.colors).Where(each => string.IsNullOrEmpty(searchContent) || each.displayName.Contains(searchContent)))
            {
                Color reverseColor = ReverseColor(colorEntry.color);
                bool isSelected = selectedColor == colorEntry.color;
                Button button = new Button(() =>
                {
                    property.colorValue = colorEntry.color;
                    property.serializedObject.ApplyModifiedProperties();
                    onValueChanged.Invoke(colorEntry.color);
                })
                {
                    tooltip = colorEntry.displayName,
                    style =
                    {
                        backgroundColor = colorEntry.color,
                        width = 20,
                        height = 20,
                        borderTopWidth = isSelected? 1: 0,
                        borderBottomWidth = isSelected? 1: 0,
                        borderLeftWidth = isSelected? 1: 0,
                        borderRightWidth = isSelected? 1: 0,
                        borderTopColor = reverseColor,
                        borderBottomColor = reverseColor,
                        borderLeftColor = reverseColor,
                        borderRightColor = reverseColor,
                    },
                };
                colorsContainer.Add(button);
            }
        }

        private static Color ReverseColor(Color oriColor)
        {
            Color.RGBToHSV(oriColor, out float h, out float s, out float v);
            float negativeH = (h + 0.5f) % 1f;
            return Color.HSVToRGB(negativeH, s, v);
        }
    }
}
#endif
