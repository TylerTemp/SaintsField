#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.UIToolkitElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.ColorPalette.UIToolkit
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public class CleanableTextInputTypeAhead: VisualElement
    {
        private bool _hoverOptions;
        private bool _focused;

        private readonly CleanableTextInput _cleanableTextInput;
        private readonly VisualElement _pop;

        public CleanableTextInputTypeAhead(SerializedProperty colorInfoLabelsProp, ScrollView root,  SerializedProperty colorInfoArray)
        {
            _pop = CreatePop();

            _cleanableTextInput = new CleanableTextInput();
            _cleanableTextInput.TextField.RegisterCallback<FocusInEvent>(_ =>
            {
                // Debug.Log("Focused");
                _focused = true;

                SetupTypeAhead(colorInfoLabelsProp, root, colorInfoArray);
            });
            _cleanableTextInput.TextField.RegisterCallback<BlurEvent>(OnBlurEvent);

            _cleanableTextInput.TextField.RegisterValueChangedCallback(e =>
            {
                _highlightLabel = "";
                FillOptions(e.newValue, colorInfoLabelsProp, root, colorInfoArray);
            });

            _cleanableTextInput.TextField.RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode is KeyCode.Return or KeyCode.KeypadEnter || e.character == '\n')
                {
                    e.StopPropagation();
                    e.PreventDefault();
                    int getIndex = GetHighlightedIndex();
                    if (getIndex != -1)
                    {
                        if(OnInputOption(_buttonItems[getIndex].LabelText, colorInfoLabelsProp))
                        {
                            _highlightLabel = "";
                            _cleanableTextInput.TextField.value = "";
                            // e.StopPropagation();
                            // _cleanableTextInput.TextField.UnregisterCallback<BlurEvent>(OnBlurEvent);
                            // FillOptions("", colorInfoLabelsProp, colorInfoArray);
                            // _cleanableTextInput.TextField.RegisterCallback<BlurEvent>(OnBlurEvent);
                        }

                        return;
                    }

                    string newValue = _cleanableTextInput.TextField.value;
                    if (!string.IsNullOrEmpty(newValue))
                    {
                        if(OnInputOption(newValue, colorInfoLabelsProp))
                        {
                            _highlightLabel = "";
                            _cleanableTextInput.TextField.value = "";

                            // _cleanableTextInput.TextField.UnregisterCallback<BlurEvent>(OnBlurEvent);
                            // FillOptions("", colorInfoLabelsProp, colorInfoArray);
                            // _cleanableTextInput.TextField.RegisterCallback<BlurEvent>(OnBlurEvent);
                            // SetupTypeAhead(colorInfoLabelsProp, root, colorInfoArray);
                            // e.StopPropagation();
                        }
                    }

                    return;
                }

                if (e.keyCode == KeyCode.Escape)
                {
                    _highlightLabel = "";
                    foreach (ButtonItem buttonItem in _buttonItems)
                    {
                        buttonItem.SetHighlighted(false);
                    }
                }
            });

            _cleanableTextInput.TextField.RegisterCallback<NavigationMoveEvent>(e =>
            {
                int count = _buttonItems.Count;
                if (count == 0)
                {
                    return;
                }

                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (e.direction)
                {
                    case NavigationMoveEvent.Direction.Up:
                    {
                        int index = GetHighlightedIndex();
                        if (index is -1 or 0)
                        {
                            var last = _buttonItems[count - 1];
                            _highlightLabel = last.LabelText;
                        }
                        else
                        {
                            _highlightLabel = _buttonItems[index - 1].LabelText;
                        }

                        foreach (ButtonItem buttonItem in _buttonItems)
                        {
                            buttonItem.SetHighlighted(buttonItem.LabelText == _highlightLabel);
                        }
                    }
                        break;
                    case NavigationMoveEvent.Direction.Down:
                    {
                        int index = GetHighlightedIndex();
                        if(index == -1 || index == count - 1)
                        {
                            _highlightLabel = _buttonItems[0].LabelText;
                        }
                        else
                        {
                            _highlightLabel = _buttonItems[index + 1].LabelText;
                        }

                        foreach (ButtonItem buttonItem in _buttonItems)
                        {
                            buttonItem.SetHighlighted(buttonItem.LabelText == _highlightLabel);
                        }
                    }
                        break;
                }
            });

            _pop.RegisterCallback<MouseEnterEvent>(_ => _hoverOptions = true);
            _pop.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                _hoverOptions = false;
                if (!_focused)
                {
                    Debug.Log("_pop.RemoveFromHierarchy");
                    _pop.RemoveFromHierarchy();
                }
            });

            Add(_cleanableTextInput);

            schedule.Execute(() =>
            {
                if (_focused)
                {
                    PosTypeAhead(root);
                }
            }).Every(100);
        }

        private void OnBlurEvent(BlurEvent evt)
        {
            _focused = false;

            if (!_hoverOptions)
            {
                Debug.Log("_pop.RemoveFromHierarchy _cleanableTextInput.BlurEvent");
                _pop.RemoveFromHierarchy();
            }
        }

        private void SetupTypeAhead(SerializedProperty colorInfoLabelsProp, ScrollView root,  SerializedProperty colorInfoArray)
        {
            PosTypeAhead(root);

            FillOptions(_cleanableTextInput.TextField.value, colorInfoLabelsProp, root, colorInfoArray);

            Debug.Log("_pop Add");
            root.Add(_pop);
        }

        private void PosTypeAhead(ScrollView root)
        {
            Vector2 worldAnchor = new Vector2(_cleanableTextInput.worldBound.xMin, _cleanableTextInput.worldBound.yMax);
            Vector2 localAnchor = root.contentContainer.WorldToLocal(worldAnchor);
            if (_pop.style.top != localAnchor.y)
            {
                _pop.style.top = localAnchor.y;
            }

            if (_pop.style.left != localAnchor.x)
            {
                _pop.style.left = localAnchor.x;
            }
        }

        private readonly List<ButtonItem> _buttonItems = new List<ButtonItem>();
        private string _highlightLabel = "";

        private int GetHighlightedIndex()
        {
            return _buttonItems.FindIndex(each => each.LabelText == _highlightLabel);
        }

        private void FillOptions(string search, SerializedProperty colorInfoLabelsProp, ScrollView root,
            SerializedProperty colorInfoArray)
        {
            string[] searchLowerPieces = search.ToLower().Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            HashSet<string> curLabels = new HashSet<string>(GetLabels(colorInfoLabelsProp))
            {
                "",  // filter out empty labels too
            };

            IEnumerable<string> options = Enumerable.Range(0, colorInfoArray.arraySize)
                .Select(i => colorInfoArray.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(ColorPaletteArray.ColorInfo.labels)))
                .SelectMany(GetLabels)
                .Except(curLabels)
                .OrderBy(each => each.ToLower())
                .Distinct()
                .Where(label => Search(searchLowerPieces, label));

            _pop.Clear();
            _buttonItems.Clear();
            HashSet<string> alreadyOptions = new HashSet<string>();
            foreach (string option in options)
            {
                // ReSharper disable once InvertIf
                if(alreadyOptions.Add(option))
                {
                    ButtonItem item = new ButtonItem(option);
                    item.Button.clicked += () =>
                    {
                        // Debug.Log($"Selected: {option}");
                        OnInputOption(option, colorInfoLabelsProp);
                    };
                    if (option == _highlightLabel)
                    {
                        item.SetHighlighted(true);
                    }
                    _pop.Add(item);
                    _buttonItems.Add(item);
                }
            }
        }

        private static IEnumerable<string> GetLabels(SerializedProperty colorInfoLabelsProp)
        {
            return Enumerable.Range(0, colorInfoLabelsProp.arraySize)
                .Select(i => colorInfoLabelsProp.GetArrayElementAtIndex(i).stringValue);
        }

        private static bool OnInputOption(string value, SerializedProperty colorInfoLabelsProp)
        {
            if (GetLabels(colorInfoLabelsProp).Any(each => each == value))
            {
                return false;
            }

            int index = colorInfoLabelsProp.arraySize;
            colorInfoLabelsProp.arraySize++;
            SerializedProperty newLabelProp = colorInfoLabelsProp.GetArrayElementAtIndex(index);
            newLabelProp.stringValue = value;
            newLabelProp.serializedObject.ApplyModifiedProperties();
            return true;
        }

        private static bool Search(IReadOnlyList<string> searchLowers, string label)
        {
            if (searchLowers.Count == 0)
            {
                return true;
            }

            string labelLower = label.ToLower();
            return searchLowers.All(search => labelLower.Contains(search));
        }

        private static VisualElement CreatePop()
        {
            VisualTreeAsset popPanel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("25abb1add80283f4aa3f5dbf001b5ba2"));
            VisualElement pop = popPanel.CloneTree().Q<VisualElement>("pop-root");

            // for (int index = 0; index < 10; index++)
            // {
            //     ButtonItem buttonItem = new ButtonItem($"Test Item {index}");
            //     pop.Add(buttonItem);
            // }

            return pop;
        }
    }
}
#endif
