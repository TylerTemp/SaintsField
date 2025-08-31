#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.ColorPalette;
using SaintsField.Editor.UIToolkitElements;
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
        private static string ToggleButtonName(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__ColorPalette_ToggleButton";
        private static string BelowRootName(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__ColorPalette_Below";
        private static string TypeAheadName(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__ColorPalette_TypeAhead";
        private static string ColorButtonsName(SerializedProperty property, int index) => $"{property.propertyPath}_{index}__ColorPalette_ColorButtons";

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
                name = ToggleButtonName(property, index),
            };
        }

        protected override VisualElement CreateBelowUIToolkit(SerializedProperty property,
            ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, FieldInfo info, object parent)
        {
            if (!EnsureColorPaletteArray())
            {
                return new HelpBox(ErrorMessageMissingPalette, HelpBoxMessageType.Warning);
            }
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
                name = BelowRootName(property, index),
            };

            SearchTypeAhead searchTypeAhead = new SearchTypeAhead(container)
            {
                name = TypeAheadName(property, index),
            };
            searchTypeAhead.CleanableTextInput.TextField.style.maxWidth = StyleKeyword.Null;
            root.Add(searchTypeAhead);

            VisualElement colors = new VisualElement
            {
                style =
                {
                    // display = DisplayStyle.None,
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                    justifyContent = Justify.FlexEnd,
                },
                name = ColorButtonsName(property, index),
            };
            root.Add(colors);

            return root;
        }

        // private readonly List<SaintsField.ColorPalette> _colorPalettes = new List<SaintsField.ColorPalette>();
        // private class PaletteSelectorInfo
        // {
        //     public IReadOnlyList<SaintsField.ColorPalette> SelectedPalettes;
        //     public List<SaintsField.ColorPalette> AllPalettes;
        // }

        private Color _color;

        protected override void OnAwakeUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute, int index,
            IReadOnlyList<PropertyAttribute> allAttributes, VisualElement container, Action<object> onValueChangedCallback, FieldInfo info, object parent)
        {
            _color = property.colorValue;

            VisualElement belowRoot = container.Q<VisualElement>(name: BelowRootName(property, index));
            if (belowRoot == null)
            {
                return;
            }

            ColorPaletteAttribute colorPaletteAttribute = (ColorPaletteAttribute) saintsAttribute;

            container.Q<ToolbarToggle>(name: ToggleButtonName(property, index))
                .RegisterCallback<ClickEvent>(_ => belowRoot.style.display = belowRoot.style.display == DisplayStyle.None? DisplayStyle.Flex: DisplayStyle.None);

            SearchTypeAhead searchTypeAhead = container.Q<SearchTypeAhead>(name: TypeAheadName(property, index));
            searchTypeAhead.PopClosedEvent.AddListener(() => belowRoot.style.minHeight = StyleKeyword.Null);
            searchTypeAhead.GetOptionsFunc = () =>
            {
                if (!EnsureColorPaletteArray())
                {
                    belowRoot.style.minHeight = StyleKeyword.Null;
                    return Array.Empty<string>();
                }

                // string[] searchLower = searchTypeAhead.CleanableTextInput.TextField.value.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string[] searchLower;
                string curSearch = searchTypeAhead.CleanableTextInput.TextField.value;
                if (curSearch.EndsWith(' ') || string.IsNullOrWhiteSpace(curSearch))
                {
                    searchLower = Array.Empty<string>();
                }
                else
                {
                    searchLower = new[]
                    {
                        curSearch.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries).Last(),
                    };
                }

                List<string> labels = new List<string>();
                List<string> colors = new List<string>();
                (HashSet<string> filterLabels, IEnumerable<ColorPaletteArray.ColorInfo> colorInfos) = FilterOutColorInfo(colorPaletteAttribute, property, info, parent);
                foreach (ColorPaletteArray.ColorInfo each in colorInfos)
                {
                    labels.AddRange(each.labels);
                    colors.Add($"#{ColorUtility.ToHtmlStringRGBA(each.color)}");
                }

                labels.Sort((a, b) => string.Compare(a.ToLower(), b.ToLower(), StringComparison.Ordinal));

                labels.AddRange(colors);

                string[] r = labels
                    .Where(each => CleanableTextInputTypeAhead.Search(searchLower, each.ToLower()))
                    .Except(filterLabels)
                    .Distinct()
                    .ToArray();

                belowRoot.style.minHeight = (r.Length + 2) * SingleLineHeight;
                // belowRoot.style.backgroundColor = Color.green;

                return r;
            };
            searchTypeAhead.OnInputOptionTypeAheadFunc = value =>
            {
                string curValue = searchTypeAhead.CleanableTextInput.TextField.value;
                if (curValue.EndsWith(' ') || string.IsNullOrWhiteSpace(curValue))
                {
                    searchTypeAhead.CleanableTextInput.TextField.value = value + " ";
                }
                else
                {
                    searchTypeAhead.CleanableTextInput.TextField.value = string.Join(' ', curValue.Split(' ', StringSplitOptions.RemoveEmptyEntries).SkipLast(1).Append(value)) + " ";
                }

#if UNITY_2022_3_OR_NEWER
                searchTypeAhead.CleanableTextInput.TextField.cursorIndex = searchTypeAhead.CleanableTextInput.TextField.selectIndex = searchTypeAhead.CleanableTextInput.TextField.value.Length;
#endif
                return false;
            };
            searchTypeAhead.CleanableTextInput.TextField.style.minWidth = StyleKeyword.None;
            searchTypeAhead.CleanableTextInput.TextField.style.maxWidth = StyleKeyword.None;
            searchTypeAhead.CleanableTextInput.TextField.RegisterValueChangedCallback(_ =>
                RefreshColorButtons(container, colorPaletteAttribute, property, index, onValueChangedCallback, info, parent));

            // PaletteSelectorInfo colorPaletteInfo = (PaletteSelectorInfo) dropdownButton.userData;
            // ColorPaletteAttribute colorPaletteAttribute = (ColorPaletteAttribute) saintsAttribute;
            // IReadOnlyList<ColorPaletteArray.ColorInfo> colorInfos = FillColorPalettes(colorPaletteAttribute.ColorPaletteSources, property, info, parent);

            // find the init color palette
            // Color color = property.colorValue;
            // ColorPaletteArray.ColorInfo initColorPalette =
            //     colorInfos.FirstOrDefault(colorPalette =>
            //         colorPalette.color == color
            //     );

            // ColorPaletteRegister.OnColorPalettesChanged.AddListener(FillColorPalettesParamless);
            // belowRoot.RegisterCallback<DetachFromPanelEvent>(_ => ColorPaletteRegister.OnColorPalettesChanged.RemoveListener(FillColorPalettesParamless));

            RefreshColorButtons(container, colorPaletteAttribute, property, index, onValueChangedCallback, info, parent);
            // return;

            // void FillColorPalettesParamless()
            // {
            //     // if (FillColorPalettes(colorPaletteInfo.AllPalettes, colorPaletteAttribute.ColorPaletteSources, property,
            //     //         info, parent))
            //     // {
            //     //     RefreshColorButtons(container, property, onValueChangedCallback);
            //     // }
            // }
        }

        protected override void OnUpdateUIToolkit(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container, Action<object> onValueChanged, FieldInfo info)
        {
            if (!_colorPaletteArray)
            {
                return;
            }
            // ReSharper disable once InvertIf
            if(property.colorValue != _color)
            {
                _color = property.colorValue;

                object parent = SerializedUtils.GetFieldInfoAndDirectParent(property).parent;

                RefreshColorButtons(container, (ColorPaletteAttribute) saintsAttribute, property, index, onValueChanged, info, parent);
            }
            // RefreshColorButtons(container, property, onValueChanged);
        }

        protected override void OnValueChanged(SerializedProperty property, ISaintsAttribute saintsAttribute, int index, VisualElement container,
            FieldInfo info, object parent, Action<object> onValueChangedCallback, object newValue)
        {
            if (!_colorPaletteArray)
            {
                return;
            }

            _color = (Color)newValue;
            RefreshColorButtons(container, (ColorPaletteAttribute) saintsAttribute, property, index, onValueChangedCallback, info, parent);
        }

        private static void RefreshColorButtons(VisualElement container, ColorPaletteAttribute colorPaletteAttribute, SerializedProperty property, int index, Action<object> onValueChanged, MemberInfo info, object parent)
        {
            SearchTypeAhead searchTypeAhead = container.Q<SearchTypeAhead>(name: TypeAheadName(property, index));
            if (searchTypeAhead == null)
            {
                return;
            }

            VisualElement colorsContainer = container.Q<VisualElement>(name: ColorButtonsName(property, index));
            colorsContainer.Clear();

            Color selectedColor = property.colorValue;

            ColorPaletteArray.ColorInfo[] allPalettes = FilterOutColorInfo(
                colorPaletteAttribute, property, info, parent).colorInfos.ToArray();

            foreach (DisplayColorEntry displayColorEntry in GetDisplayColorEntries(selectedColor, searchTypeAhead.CleanableTextInput.TextField.value, allPalettes))
            {
                ColorPaletteArray.ColorInfo colorEntry = displayColorEntry.ColorEntry;
                Color reverseColor = displayColorEntry.ReversedColor;
                bool isSelected = displayColorEntry.IsSelected;
                Button button = new Button(() =>
                {
                    property.colorValue = colorEntry.color;
                    property.serializedObject.ApplyModifiedProperties();
                    onValueChanged.Invoke(colorEntry.color);
                })
                {
                    tooltip = string.Join("\n", colorEntry.labels),
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

            bool anySelected = _colorPaletteArray.Any(eachPalettes =>
                eachPalettes.color == selectedColor);
            ToolbarToggle toggleButton = container.Q<ToolbarToggle>(name: ToggleButtonName(property, index));
            Texture2D icon = anySelected? _colorPaletteIcon: _colorPaletteWarningIcon;
            if (toggleButton.style.backgroundImage != icon)
            {
                toggleButton.style.backgroundImage = icon;
            }
        }
    }
}
#endif
