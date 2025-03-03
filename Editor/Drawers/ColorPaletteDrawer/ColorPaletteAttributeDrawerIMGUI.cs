using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.ColorPalette;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SaintsField.Editor.Drawers.ColorPaletteDrawer
{
    public partial class ColorPaletteAttributeDrawer
    {
        private class ButtonBackgroundToggle: IDisposable
        {
            private readonly Color _originalColor;

            public ButtonBackgroundToggle(bool on)
            {
                _originalColor = GUI.backgroundColor;
                GUI.backgroundColor = on? EColor.EditorEmphasized.GetColor(): GUI.backgroundColor;
            }

            public void Dispose()
            {
                GUI.backgroundColor = _originalColor;
            }

        }

        private class PaletteSelectorInfoImGui
        {
            public bool Expanded;

            public IReadOnlyList<SaintsField.ColorPalette> SelectedPalettes;
            public List<SaintsField.ColorPalette> AllPalettes;
            public string SearchText;
        }

        private static readonly Dictionary<string, PaletteSelectorInfoImGui> ImGuiCache = new Dictionary<string, PaletteSelectorInfoImGui>();

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            ColorPaletteAttribute colorPaletteAttribute = (ColorPaletteAttribute) saintsAttribute;

            string key = SerializedUtils.GetUniqueId(property);
            // ReSharper disable once InvertIf
            if(!ImGuiCache.TryGetValue(key, out PaletteSelectorInfoImGui paletteSelectorInfo))
            {
                ImGuiCache[key] = paletteSelectorInfo = new PaletteSelectorInfoImGui
                {
                    Expanded = false,
                    SelectedPalettes = new List<SaintsField.ColorPalette>(),
                    AllPalettes = new List<SaintsField.ColorPalette>(),
                    SearchText = "",
                };

                FillColorPalettes(paletteSelectorInfo.AllPalettes, colorPaletteAttribute.ColorPaletteSources,
                    property,
                    info, parent);

                SaintsField.ColorPalette initColorPalette =
                    paletteSelectorInfo.AllPalettes.FirstOrDefault(colorPalette =>
                        colorPalette.colors.Any(each => each.color == property.colorValue)
                    );

                if (initColorPalette != null)
                {
                    paletteSelectorInfo.SelectedPalettes = new[] { initColorPalette };
                }
                else if(paletteSelectorInfo.SelectedPalettes.Count == 0)
                {
                    paletteSelectorInfo.SelectedPalettes = paletteSelectorInfo.AllPalettes;
                }

                void FillColorPalettesParamless()
                {
                    FillColorPalettes(paletteSelectorInfo.AllPalettes, colorPaletteAttribute.ColorPaletteSources,
                        property,
                        info, parent);
                }

                ColorPaletteRegister.OnColorPalettesChanged.AddListener(FillColorPalettesParamless);

                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
                    ColorPaletteRegister.OnColorPalettesChanged.RemoveListener(FillColorPalettesParamless);
                    ImGuiCache.Remove(key);
                });
            }

            return SingleLineHeight;
        }

        private static GUIStyle _buttonStyle;

        protected override bool DrawPostFieldImGui(Rect position, Rect fullRect, SerializedProperty property,
            GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGUIPayload, FieldInfo info, object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            if(!ImGuiCache.TryGetValue(key, out PaletteSelectorInfoImGui paletteSelectorInfo))
            {
                return false;
            }

            Color selectedColor = property.colorValue;
            bool anySelected = paletteSelectorInfo.AllPalettes.Any(eachPalettes =>
                eachPalettes.colors.Any(eachColorEntry => eachColorEntry.color == selectedColor));

            Texture2D icon;
            if (anySelected)
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                if (_colorPaletteIcon is null)
                {
                    _colorPaletteIcon = Util.LoadResource<Texture2D>("color-palette.png");
                }
                icon = _colorPaletteIcon;
            }
            else
            {
                // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
                if (_colorPaletteWarningIcon is null)
                {
                    _colorPaletteWarningIcon = Util.LoadResource<Texture2D>("color-palette-warning.png");
                }
                icon = _colorPaletteWarningIcon;
            }


            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_buttonStyle is null)
            {
                _buttonStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    padding = new RectOffset(2, 2, 2, 2),
                };
            }

            // if (GUI.Button(position, icon, _buttonStyle))
            using(new ButtonBackgroundToggle(paletteSelectorInfo.Expanded))
            {
                if (GUI.Button(position, icon, _buttonStyle))
                {
                    paletteSelectorInfo.Expanded = !paletteSelectorInfo.Expanded;
                }
            }

            return true;
        }

        protected override bool WillDrawBelow(SerializedProperty property, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if(!ImGuiCache.TryGetValue(key, out PaletteSelectorInfoImGui paletteSelectorInfo))
            {
                return false;
            }
            return paletteSelectorInfo.Expanded;
        }

        private const int ButtonPaddding = 1;

        protected override float GetBelowExtraHeight(SerializedProperty property, GUIContent label, float width,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if(!ImGuiCache.TryGetValue(key, out PaletteSelectorInfoImGui paletteSelectorInfo))
            {
                return 0;
            }

            if (!paletteSelectorInfo.Expanded)
            {
                return 0;
            }

            float dropdownHeight = SingleLineHeight;
            int colorCount = GetDisplayColorEntries(property.colorValue, paletteSelectorInfo.SearchText,
                paletteSelectorInfo.SelectedPalettes).Count();
            if (colorCount == 0)
            {
                return dropdownHeight + SingleLineHeight;
            }

            float useWidth = width - 2;
            const int buttonWidthSpace = ButtonPaddding * 2 + ColorButtonSize;
            int buttonRowCount = Mathf.FloorToInt(useWidth / buttonWidthSpace);
            int rowCount = Mathf.CeilToInt(colorCount / (float) buttonRowCount);
            return dropdownHeight + rowCount * buttonWidthSpace + 2;  // 2 is for padding
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            string key = SerializedUtils.GetUniqueId(property);
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if(!ImGuiCache.TryGetValue(key, out PaletteSelectorInfoImGui paletteSelectorInfo))
            {
                return position;
            }

            EditorGUI.DrawRect(position, EColor.EditorEmphasized.GetColor());

            (Rect dropdownRectRaw, Rect colorPickerRectRaw) = RectUtils.SplitHeightRect(position, SingleLineHeight);
            Rect dropdownRect = new Rect(dropdownRectRaw)
            {
                y = dropdownRectRaw.y + 1,
                height = dropdownRectRaw.height - 2,
                x = dropdownRectRaw.x + 1,
                width = dropdownRectRaw.width - 2,
            };

            Rect dropdownRectLeft = EditorGUI.PrefixLabel(dropdownRect, new GUIContent("Palette"));
            (Rect dropdownRectButton, Rect dropdownRectSearch) = RectUtils.SplitWidthRect(dropdownRectLeft, dropdownRectLeft.width * 0.7f);
            if (EditorGUI.DropdownButton(dropdownRectButton, new GUIContent(string.Join(", ", paletteSelectorInfo.SelectedPalettes.Select(each => each.displayName))), FocusType.Keyboard))
            {
                AdvancedDropdownMetaInfo dropdownMetaInfo = GetMetaInfo(paletteSelectorInfo.SelectedPalettes, paletteSelectorInfo.AllPalettes, true);
                Vector2 size = AdvancedDropdownUtil.GetSizeIMGUI(dropdownMetaInfo.DropdownListValue, position.width);
                SaintsAdvancedDropdownIMGUI dropdown = new SaintsAdvancedDropdownIMGUI(
                    dropdownMetaInfo.DropdownListValue,
                    size,
                    position,
                    new AdvancedDropdownState(),
                    curItem =>
                    {
                        if (curItem is null)
                        {
                            // Debug.Log("Open Editor");
                            ColorPaletteMenu.OpenWindow();
                            return;
                        }

                        paletteSelectorInfo.SelectedPalettes = (IReadOnlyList<SaintsField.ColorPalette>) curItem;
                    },
                    _ => null);
                dropdown.Show(dropdownRect);
                dropdown.BindWindowPosition();
            }

            paletteSelectorInfo.SearchText = EditorGUI.TextField(dropdownRectSearch, paletteSelectorInfo.SearchText);

            Rect colorPickerRect = new Rect(colorPickerRectRaw)
            {
                y = colorPickerRectRaw.y + 1,
                height = colorPickerRectRaw.height - 2,
                x = colorPickerRectRaw.x + 1,
                width = colorPickerRectRaw.width - 2,
            };

            DisplayColorEntry[] displayColorEntries = GetDisplayColorEntries(property.colorValue, paletteSelectorInfo.SearchText,
                paletteSelectorInfo.SelectedPalettes).ToArray();
            float useWidth = colorPickerRect.width;
            const int buttonWidthSpace = ButtonPaddding * 2 + ColorButtonSize;
            int buttonRowCount = Mathf.FloorToInt(useWidth / buttonWidthSpace);
            const float buttonHeight = ColorButtonSize + ButtonPaddding;
            const float buttonWidth = ColorButtonSize + ButtonPaddding;
            for (int colorIndex = 0; colorIndex < displayColorEntries.Length; colorIndex++)
            {
                int row = colorIndex / buttonRowCount;
                int col = colorIndex % buttonRowCount;
                Rect buttonRect = new Rect(colorPickerRect.x + useWidth - (col + 1) * buttonWidthSpace, colorPickerRect.y + row * buttonHeight, buttonWidth, buttonHeight);
                DisplayColorEntry displayColorEntry = displayColorEntries[colorIndex];

                EditorGUI.DrawRect(new Rect(buttonRect.x + ButtonPaddding, buttonRect.y + ButtonPaddding, ColorButtonSize, ColorButtonSize), displayColorEntry.ColorEntry.color);
                // frame
                if (displayColorEntry.IsSelected)
                {
                    const float frameWidth = 2;

                    Rect top = new Rect(buttonRect)
                    {
                        height = frameWidth,
                    };
                    EditorGUI.DrawRect(top, displayColorEntry.ReversedColor);

                    Rect bottom = new Rect(buttonRect)
                    {
                        y = buttonRect.y + buttonRect.height - frameWidth,
                        height = frameWidth,
                    };
                    EditorGUI.DrawRect(bottom, displayColorEntry.ReversedColor);

                    Rect left = new Rect(buttonRect)
                    {
                        width = frameWidth,
                    };
                    EditorGUI.DrawRect(left, displayColorEntry.ReversedColor);

                    Rect right = new Rect(buttonRect)
                    {
                        x = buttonRect.x + buttonRect.width - frameWidth,
                        width = frameWidth,
                    };
                    EditorGUI.DrawRect(right, displayColorEntry.ReversedColor);
                }
                if (GUI.Button(buttonRect, new GUIContent(GUIContent.none)
                    {
                        tooltip = displayColorEntry.ColorEntry.displayName,
                    }, GUIStyle.none))
                {
                    property.colorValue = displayColorEntry.ColorEntry.color;
                    onGuiPayload.SetValue(property.colorValue);
                }
            }

            return new Rect(position)
            {
                y = position.y + position.height,
                height = 0,
            };
        }
    }
}
