#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.ColorPalette;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace SaintsField.Editor.Drawers.ColorPaletteDrawer
{
    public partial class ColorPaletteAttributeDrawer
    {
        private static string ToggleButtonName(SerializedProperty property) => $"{property.propertyPath}__ColorPalette_ToggleButton";
        private static string BelowRootName(SerializedProperty property) => $"{property.propertyPath}__ColorPalette_Below";
        private static string DropdownName(SerializedProperty property) => $"{property.propertyPath}__ColorPalette_Dropdown";
        private static string SearchInputName(SerializedProperty property) => $"{property.propertyPath}__ColorPalette_SearchInput";
        private static string ColorButtonsName(SerializedProperty property) => $"{property.propertyPath}__ColorPalette_ColorButtons";

        protected override VisualElement CreatePostFieldUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, FieldInfo info, object parent)
        {
            _colorPaletteIcon = Util.LoadResource<Texture2D>("color-palette.png");
            _colorPaletteWarningIcon = Util.LoadResource<Texture2D>("color-palette-warning.png");
            return new ToolbarToggle
            {
                style =
                {
                    backgroundImage = _colorPaletteIcon,
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
                    display = DisplayStyle.None,
                    backgroundColor = EColor.EditorEmphasized.GetColor(),
                    paddingTop = 2,
                    paddingBottom = 2,
                    paddingLeft = 2,
                    paddingRight = 4,
                },
                name = BelowRootName(property),
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

            ToolbarSearchField searchField = new ToolbarSearchField
            {
                style =
                {
                    width = Length.Percent(20),
                },
                name = SearchInputName(property),
            };

            // TextField searchField = new TextField
            // {
            //     style =
            //     {
            //         width = Length.Percent(20),
            //     },
            //     name = SearchInputName(property),
            // };
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

        private Color _color;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            _color = property.colorValue;

            VisualElement belowRoot = container.Q<VisualElement>(name: BelowRootName(property));

            container.Q<ToolbarToggle>(name: ToggleButtonName(property))
                .RegisterCallback<ClickEvent>(_ => belowRoot.style.display = belowRoot.style.display == DisplayStyle.None? DisplayStyle.Flex: DisplayStyle.None);

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
            else if(colorPaletteInfo.SelectedPalettes.Count == 0)
            {
                colorPaletteInfo.SelectedPalettes = colorPaletteInfo.AllPalettes;
            }

            dropdownButton.ButtonElement.clicked += () =>
            {
                AdvancedDropdownMetaInfo dropdownMetaInfo = GetMetaInfo(colorPaletteInfo.SelectedPalettes, colorPaletteInfo.AllPalettes, false);

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

            ToolbarSearchField searchInput = container.Q<ToolbarSearchField>(name: SearchInputName(property));
            searchInput.RegisterValueChangedCallback(_ =>
                RefreshColorButtons(container, property, onValueChangedCallback));

            ColorPaletteRegister.OnColorPalettesChanged.AddListener(FillColorPalettesParamless);
            belowRoot.RegisterCallback<DetachFromPanelEvent>(_ => ColorPaletteRegister.OnColorPalettesChanged.RemoveListener(FillColorPalettesParamless));

            RefreshColorButtons(container, property, onValueChangedCallback);
            return;

            void FillColorPalettesParamless()
            {
                if (FillColorPalettes(colorPaletteInfo.AllPalettes, colorPaletteAttribute.ColorPaletteSources, property,
                        info, parent))
                {
                    RefreshColorButtons(container, property, onValueChangedCallback);
                }
            }
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            // ReSharper disable once InvertIf
            if(property.colorValue != _color)
            {
                _color = property.colorValue;
                RefreshColorButtons(container, property, onValueChanged);
            }
            // RefreshColorButtons(container, property, onValueChanged);
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            _color = (Color)newValue;
            RefreshColorButtons(container, property, onValueChangedCallback);
        }

        private static void RefreshColorButtons(VisualElement container, SerializedProperty property, Action<object> onValueChanged)
        {
            UIToolkitUtils.DropdownButtonField dropdownButton = container.Q<UIToolkitUtils.DropdownButtonField>(name: DropdownName(property));
            PaletteSelectorInfo paletteSelectorInfo = (PaletteSelectorInfo) dropdownButton.userData;

            dropdownButton.ButtonLabelElement.text = string.Join(",", paletteSelectorInfo.SelectedPalettes.Select(each => each.displayName));

            ToolbarSearchField searchInput = container.Q<ToolbarSearchField>(name: SearchInputName(property));
            string searchContent = searchInput.value.Trim();

            VisualElement colorsContainer = container.Q<VisualElement>(name: ColorButtonsName(property));
            colorsContainer.Clear();

            Color selectedColor = property.colorValue;

            foreach (DisplayColorEntry displayColorEntry in GetDisplayColorEntries(selectedColor, searchContent, paletteSelectorInfo.SelectedPalettes))
            {
                SaintsField.ColorPalette.ColorEntry colorEntry = displayColorEntry.ColorEntry;
                Color reverseColor = displayColorEntry.ReversedColor;
                bool isSelected = displayColorEntry.IsSelected;
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
                        width = ColorButtonSize,
                        height = ColorButtonSize,
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

            bool anySelected = paletteSelectorInfo.AllPalettes.Any(eachPalettes =>
                eachPalettes.colors.Any(eachColorEntry => eachColorEntry.color == selectedColor));
            ToolbarToggle toggleButton = container.Q<ToolbarToggle>(name: ToggleButtonName(property));
            Texture2D icon = anySelected? _colorPaletteIcon: _colorPaletteWarningIcon;
            if (toggleButton.style.backgroundImage != icon)
            {
                toggleButton.style.backgroundImage = icon;
            }
        }
    }
}
#endif
