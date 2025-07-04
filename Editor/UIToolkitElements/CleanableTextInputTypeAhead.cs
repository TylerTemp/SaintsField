#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    // ReSharper disable once PartialTypeWithSinglePart
    public abstract partial class CleanableTextInputTypeAhead: VisualElement
    {
        private bool _hoverOptions;
        private bool _focused;

        public readonly CleanableTextInput CleanableTextInput;
        public readonly UnityEvent PopClosedEvent = new UnityEvent();
        protected readonly VisualElement Pop;

        public CleanableTextInputTypeAhead(): this(null){}

        public CleanableTextInputTypeAhead(VisualElement root)
        {
            Pop = CreatePop();

            CleanableTextInput = new CleanableTextInput();
            CleanableTextInput.TextField.RegisterCallback<FocusInEvent>(_ =>
            {
                // Debug.Log("Focused");
                _focused = true;

                SetupTypeAhead(root);
            });
            CleanableTextInput.TextField.RegisterCallback<BlurEvent>(OnBlurEvent);

            CleanableTextInput.TextField.RegisterValueChangedCallback(e =>
            {
                _highlightLabel = "";
                FillOptions();
            });

            CleanableTextInput.TextField.RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode is KeyCode.Return or KeyCode.KeypadEnter || e.character == '\n')
                {
                    e.StopPropagation();
                    e.PreventDefault();
                    int getIndex = GetHighlightedIndex();
                    if (getIndex != -1)
                    {
                        if(OnInputOptionTypeAhead(_buttonItems[getIndex].LabelText))
                        {
                            _highlightLabel = "";
                            CleanableTextInput.TextField.value = "";
                            // e.StopPropagation();
                            // _cleanableTextInput.TextField.UnregisterCallback<BlurEvent>(OnBlurEvent);
                            // FillOptions("", colorInfoLabelsProp, colorInfoArray);
                            // _cleanableTextInput.TextField.RegisterCallback<BlurEvent>(OnBlurEvent);
                        }

                        return;
                    }

                    string newValue = CleanableTextInput.TextField.value;
                    if (!string.IsNullOrEmpty(newValue))
                    {
                        if(OnInputOptionReturn(newValue))
                        {
                            _highlightLabel = "";
                            CleanableTextInput.TextField.value = "";

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

            CleanableTextInput.TextField.RegisterCallback<NavigationMoveEvent>(e =>
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
                            ButtonItem last = _buttonItems[count - 1];
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

            Pop.RegisterCallback<MouseEnterEvent>(_ => _hoverOptions = true);
            Pop.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                _hoverOptions = false;
                if (!_focused)
                {
                    // Debug.Log("_pop.RemoveFromHierarchy");
                    Pop.RemoveFromHierarchy();
                    PopClosedEvent.Invoke();
                }
            });

            Add(CleanableTextInput);

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
                // Debug.Log("_pop.RemoveFromHierarchy _cleanableTextInput.BlurEvent");
                Pop.RemoveFromHierarchy();
                PopClosedEvent.Invoke();
            }
        }

        private void SetupTypeAhead(VisualElement root)
        {
            FillOptions();
            PosTypeAhead(root);

            // Debug.Log("_pop Add");
            root.Add(Pop);
        }

        protected virtual void PosTypeAhead(VisualElement root)
        {
            Vector2 worldAnchor = new Vector2(CleanableTextInput.worldBound.xMin, CleanableTextInput.worldBound.yMax);
            VisualElement targetElement = root.contentContainer ?? root;
            Vector2 localAnchor = (targetElement).WorldToLocal(worldAnchor);
            if (Pop.style.top != localAnchor.y)
            {
                Pop.style.top = localAnchor.y;
            }

            if (Pop.style.left != localAnchor.x)
            {
                Pop.style.left = localAnchor.x;
            }

            // float popHeight;
            // if (double.IsNaN(_pop.resolvedStyle.height))
            // {
            //     popHeight = _curOptions.Count * SaintsPropertyDrawer.SingleLineHeight + 4;
            // }
            // else
            // {
            //     popHeight = _pop.resolvedStyle.height;
            // }
            //
            // targetElement.style.minHeight = popHeight + Mathf.Abs(localAnchor.y);
        }

        private readonly List<ButtonItem> _buttonItems = new List<ButtonItem>();
        private string _highlightLabel = "";

        private int GetHighlightedIndex()
        {
            return _buttonItems.FindIndex(each => each.LabelText == _highlightLabel);
        }

        protected abstract IReadOnlyList<string> GetOptions();

        protected IReadOnlyList<string> CurOptions = Array.Empty<string>();

        private void FillOptions()
        {
            CurOptions = GetOptions();
                // .Where(label => Search(searchLowerPieces, label.ToLower()));

            Pop.Clear();
            _buttonItems.Clear();
            HashSet<string> alreadyOptions = new HashSet<string>();
            foreach (string option in CurOptions)
            {
                // ReSharper disable once InvertIf
                if(alreadyOptions.Add(option))
                {
                    ButtonItem item = new ButtonItem(option);
                    item.Button.clicked += () =>
                    {
                        // Debug.Log($"Selected: {option}");
                        OnInputOptionTypeAhead(option);
                    };
                    if (option == _highlightLabel)
                    {
                        item.SetHighlighted(true);
                    }
                    Pop.Add(item);
                    _buttonItems.Add(item);
                }
            }
        }

        // private static IEnumerable<string> GetLabels(SerializedProperty colorInfoLabelsProp)
        // {
        //     return Enumerable.Range(0, colorInfoLabelsProp.arraySize)
        //         .Select(i => colorInfoLabelsProp.GetArrayElementAtIndex(i).stringValue);
        // }

        protected abstract bool OnInputOptionReturn(string value);
        protected abstract bool OnInputOptionTypeAhead(string value);

        public static bool Search(IReadOnlyList<string> searchLowers, string label)
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
