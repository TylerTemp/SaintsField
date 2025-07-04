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

            // public IReadOnlyList<SaintsField.ColorPalette> SelectedPalettes;
            public IReadOnlyList<ColorPaletteArray.ColorInfo> AllPalettes;
            public string SearchText;
            public ColorPaletteArray.ColorInfo Selected;
        }

        private static readonly Dictionary<string, PaletteSelectorInfoImGui> ImGuiCache = new Dictionary<string, PaletteSelectorInfoImGui>();

        protected override float GetPostFieldWidth(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            if (!EnsureColorPaletteArray())
            {
                return 0;
            }

            // ColorPaletteAttribute colorPaletteAttribute = (ColorPaletteAttribute) saintsAttribute;

            string key = SerializedUtils.GetUniqueId(property);
            // ReSharper disable once InvertIf
            if(!ImGuiCache.ContainsKey(key))
            {
                ImGuiCache[key] = new PaletteSelectorInfoImGui
                {
                    Expanded = false,
                    AllPalettes = FilterOutColorInfo(
                        (ColorPaletteAttribute)saintsAttribute, property, info, parent).colorInfos.ToArray(),
                    SearchText = "",
                    Selected = default,
                };

                NoLongerInspectingWatch(property.serializedObject.targetObject, key, () =>
                {
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
            if (!EnsureColorPaletteArray())
            {
                return false;
            }

            string key = SerializedUtils.GetUniqueId(property);
            if(!ImGuiCache.TryGetValue(key, out PaletteSelectorInfoImGui paletteSelectorInfo))
            {
                return false;
            }

            Color selectedColor = property.colorValue;
            bool anySelected = paletteSelectorInfo.AllPalettes.Any(eachPalettes => eachPalettes.color == selectedColor);

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

        protected override bool WillDrawBelow(SerializedProperty property,
            IReadOnlyList<PropertyAttribute> allAttributes, ISaintsAttribute saintsAttribute,
            int index,
            FieldInfo info,
            object parent)
        {
            if (!EnsureColorPaletteArray())
            {
                return true;
            }
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
            IReadOnlyList<PropertyAttribute> allAttributes,
            ISaintsAttribute saintsAttribute, int index, FieldInfo info, object parent)
        {
            if (!EnsureColorPaletteArray())
            {
                return ImGuiHelpBox.GetHeight(ErrorMessageMissingPalette, width, EMessageType.Error);
            }

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

            int colorCount = GetDisplayColorEntries(property.colorValue, paletteSelectorInfo.SearchText,
                paletteSelectorInfo.AllPalettes).Count();
            if (colorCount == 0)
            {
                return SingleLineHeight + SingleLineHeight;
            }

            float useWidth = width - 2;
            const int buttonWidthSpace = ButtonPaddding * 2 + ColorButtonSize;
            int buttonRowCount = Mathf.FloorToInt(useWidth / buttonWidthSpace);
            int rowCount = Mathf.CeilToInt(colorCount / (float) buttonRowCount);
            return SingleLineHeight + rowCount * buttonWidthSpace + 2;  // 2 is for padding
        }

        protected override Rect DrawBelow(Rect position, SerializedProperty property, GUIContent label,
            ISaintsAttribute saintsAttribute, int index, IReadOnlyList<PropertyAttribute> allAttributes,
            OnGUIPayload onGuiPayload, FieldInfo info, object parent)
        {
            if (!EnsureColorPaletteArray())
            {
                return ImGuiHelpBox.Draw(position, ErrorMessageMissingPalette, EMessageType.Error);
            }

            string key = SerializedUtils.GetUniqueId(property);
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if(!ImGuiCache.TryGetValue(key, out PaletteSelectorInfoImGui paletteSelectorInfo))
            {
                return position;
            }

            EditorGUI.DrawRect(position, EColor.EditorEmphasized.GetColor());

            (Rect searchRectRaw, Rect colorPickerRectRaw) = RectUtils.SplitHeightRect(position, SingleLineHeight);
            Rect searchRect = new Rect(searchRectRaw)
            {
                y = searchRectRaw.y + 1,
                height = searchRectRaw.height - 2,
                x = searchRectRaw.x + 1,
                width = searchRectRaw.width - 2,
            };

            paletteSelectorInfo.SearchText = EditorGUI.TextField(searchRect, paletteSelectorInfo.SearchText);

            Rect colorPickerRect = new Rect(colorPickerRectRaw)
            {
                y = colorPickerRectRaw.y + 1,
                height = colorPickerRectRaw.height - 2,
                x = colorPickerRectRaw.x + 1,
                width = colorPickerRectRaw.width - 2,
            };

            DisplayColorEntry[] displayColorEntries = GetDisplayColorEntries(property.colorValue, paletteSelectorInfo.SearchText,
                paletteSelectorInfo.AllPalettes).ToArray();
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
                        tooltip = string.Join("\n", displayColorEntry.ColorEntry.labels),
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
