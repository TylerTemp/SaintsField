#if UNITY_2021_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.DropdownBase;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public class SaintsTreeDropdownElement: VisualElement
    {
        // value
        // new status on or off;
        // is row click or not (row click need to close the dropdown)
        public readonly UnityEvent<object, bool, bool> OnClickedEvent = new UnityEvent<object, bool, bool>();

        // public readonly UnityEvent<TreeRowAbsElement> ScrollToElementEvent = new UnityEvent<TreeRowAbsElement>();

        private TreeRowAbsElement CurrentFocus { get; set; }
        private readonly bool _allowToggle;

        private readonly IReadOnlyList<TreeRowAbsElement> _flatList;

        public SaintsTreeDropdownElement(AdvancedDropdownMetaInfo metaInfo, bool toggle)
        {
            _allowToggle = toggle;

            // VisualElement root = new VisualElement();

            // CleanableTextInputFullWidth cleanableTextInput = new CleanableTextInputFullWidth(null);
            // Add(cleanableTextInput);
            ToolbarSearchField toolbarSearchField = new ToolbarSearchField
            {
                style =
                {
                    flexGrow = 1,
                    width = StyleKeyword.None,
                },
            };
            Add(toolbarSearchField);

            HashSet<object> curValues = metaInfo.CurValues.ToHashSet();

            OnClickedEvent.AddListener((v, isOn, _) =>
            {
                if (isOn)
                {
                    curValues.Add(v);
                }
                else
                {
                    curValues.Remove(v);
                }
            });

            TreeRowAbsElement[] treeRowElements = MakeNestedTreeRow(0,
                metaInfo.DropdownListValue,
                curValues)
                .ToArray();

            ScrollView treeContainer = new ScrollView
            {
                focusable = true,
            };

            List<TreeRowAbsElement> flatList = new List<TreeRowAbsElement>();
            foreach (TreeRowAbsElement treeRow in treeRowElements)
            {
                treeContainer.Add(treeRow);
                foreach (TreeRowAbsElement rowAbsElement in FlatTreeRow(treeRow))
                {
                    flatList.Add(rowAbsElement);
                    switch (rowAbsElement)
                    {
                        case TreeRowValueElement tr:
                            tr.OnClickedEvent.AddListener((_, _) => CurrentFocus = tr);
                            break;
                        case TreeRowFoldoutElement tf:
                            tf.RegisterValueChangedCallback(_ => CurrentFocus = tf);
                            break;
                    }
                }
            }

            _flatList = flatList;

            Add(treeContainer);

#if UNITY_6000_0_OR_NEWER
            toolbarSearchField.placeholderText = "Search";
#endif
            toolbarSearchField.RegisterCallback<NavigationMoveEvent>(evt =>
            {
                if (evt.direction == NavigationMoveEvent.Direction.Down)
                {
                    treeContainer.Focus();
                }
            });

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                toolbarSearchField.Q<TextField>().Q("unity-text-input").Focus();
                if(CurrentFocus != null)
                {
                    treeContainer.schedule
                        .Execute(() => treeContainer.ScrollTo(CurrentFocus))
                        // This delay is required for no good reason...
                        .StartingIn(100);
                }
            });

            toolbarSearchField.RegisterValueChangedCallback(evt =>
            {
                string searchText = evt.newValue;

                ListSearchToken[] searchTokens = string.IsNullOrWhiteSpace(searchText)
                    ? Array.Empty<ListSearchToken>()
                    : SerializedUtils.ParseSearch(searchText).ToArray();

                foreach (TreeRowAbsElement treeRowAbsElement in treeRowElements)
                {
                    treeRowAbsElement.OnSearch(searchTokens);
                }
            });

            // navigation
            RegisterCallback<NavigationMoveEvent>(e =>
            {
                // Debug.Log(e.direction);
                bool isUp;
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (e.direction)
                {
                    case NavigationMoveEvent.Direction.Up:
                        isUp = true;
                        break;
                    case NavigationMoveEvent.Direction.Down:
                        isUp = false;
                        break;
                    case NavigationMoveEvent.Direction.Left:
                    {
                        switch (CurrentFocus)
                        {
                            case TreeRowFoldoutElement { value: true } foldoutElement:
                                foldoutElement.value = false;
                                break;
                            case { Parent: not null }:
                            {
                                CurrentFocus = CurrentFocus.Parent;
                                // Debug.Log($"currentFocus={_currentFocus}");
                                foreach (TreeRowAbsElement treeRowAbsElement in _flatList)
                                {
                                    treeRowAbsElement.SetNavigateHighlight(CurrentFocus == treeRowAbsElement);
                                }

                                break;
                            }
                        }

                        return;
                    }
                    case NavigationMoveEvent.Direction.Right:
                    {
                        if (CurrentFocus is TreeRowFoldoutElement { value: false } foldoutElement)
                        {
                            foldoutElement.value = true;
                        }
                        return;
                    }
                    default:
                        return;
                }

                TreeRowAbsElement toFocus = null;
                if (CurrentFocus != null)
                {
                    List<TreeRowAbsElement> prevList = new List<TreeRowAbsElement>(_flatList.Count);
                    for (int index = 0; index < _flatList.Count; index++)
                    {
                        TreeRowAbsElement current = _flatList[index];
                        if (current == CurrentFocus)
                        {
                            if (isUp)
                            {
                                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                                if (prevList.Count > 0)
                                {
                                    // Debug.Log(prevList.Count);
                                    // Debug.Log($"pres: {string.Join(", ", prevList)}");
                                    toFocus = prevList.LastOrDefault(each => each.Navigateable);
                                }
                                else
                                {
                                    toFocus = _flatList.LastOrDefault(each => each.Navigateable);
                                }

                                // Debug.Log($"up to {toFocus}");
                            }
                            else
                            {
                                toFocus = _flatList.Skip(index + 1).FirstOrDefault(each => each.Navigateable)
                                          ?? _flatList.FirstOrDefault(each => each.Navigateable);
                            }

                            break;
                        }

                        // Debug.Log($"{current} -> {currentFocus}");
                        prevList.Add(current);
                    }
                }

                if (CurrentFocus == null)
                {
                    toFocus = isUp
                        ? _flatList.LastOrDefault(each => each.Navigateable)
                        : _flatList.FirstOrDefault(each => each.Navigateable);
                }

                if (toFocus != null)
                {
                    CurrentFocus = toFocus;

                    // Debug.Log($"currentFocus={_currentFocus}");

                    foreach (TreeRowAbsElement treeRowAbsElement in _flatList)
                    {
                        treeRowAbsElement.SetNavigateHighlight(toFocus == treeRowAbsElement);
                    }

                    // ScrollToElementEvent.Invoke(CurrentFocus);
                    treeContainer.ScrollTo(CurrentFocus);
                }
            }, TrickleDown.TrickleDown);
            RegisterCallback<KeyUpEvent>(e =>
            {

                if (CurrentFocus is null)
                {
                    return;
                }

                // ReSharper disable once InvertIf
                if (e.keyCode is KeyCode.Space or KeyCode.Return or KeyCode.KeypadEnter)
                {
                    switch (CurrentFocus)
                    {
                        case TreeRowFoldoutElement foldoutElement:
                            foldoutElement.value = !foldoutElement.value;
                            break;
                        case TreeRowValueElement valueElement:
                            valueElement.SetValueOn(!valueElement.IsOn);
                            valueElement.OnClickedEvent.Invoke(valueElement.IsOn, false);
                            break;
                    }
                }

            });
        }

        public int GetMaxHeight()
        {
            int result = SaintsPropertyDrawer.SingleLineHeight + 2 + 18;  // search bar height + border + scroller
            foreach (TreeRowAbsElement treeRowAbsElement in _flatList)
            {
                if (treeRowAbsElement is TreeRowSepElement)
                {
                    result += 2;
                }
                else
                {
                    result += 20;
                }
            }

            return result;
        }

        public void RefreshValues(IReadOnlyList<object> curValues)
        {
            foreach (TreeRowAbsElement treeRowAbsElement in _flatList)
            {
                // ReSharper disable once InvertIf
                if (treeRowAbsElement is TreeRowValueElement valueElement)
                {
                    bool shouldOn = curValues.Contains(valueElement.Value);
                    if (valueElement.IsOn != shouldOn)
                    {
                        valueElement.SetValueOn(shouldOn);
                    }
                }
            }
        }

        private static IEnumerable<TreeRowAbsElement> FlatTreeRow(TreeRowAbsElement treeRow)
        {
            if (treeRow is TreeRowSepElement)
            {
                yield break;
            }

            yield return treeRow;

            // ReSharper disable once InvertIf
            if (treeRow is TreeRowFoldoutElement treeRowFoldoutElement)
            {
                foreach (TreeRowAbsElement subRow in treeRowFoldoutElement.ContentChildren)
                {
                    foreach (TreeRowAbsElement flatSub in FlatTreeRow(subRow))
                    {
                        yield return flatSub;
                    }
                }
            }
        }

        private IReadOnlyList<TreeRowAbsElement> MakeNestedTreeRow(int indent, IAdvancedDropdownList dropdownLis, ICollection<object> curValues)
        {
            List<TreeRowAbsElement> result = new List<TreeRowAbsElement>(dropdownLis.Count);

            bool hasMeaningfulChild = false;
            // bool hasSelect = false;
            // int incrId = accId;
            // bool isEmptyNode = true;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (IAdvancedDropdownList dropdownItem in dropdownLis)
            {
                if (dropdownItem.isSeparator)
                {
                    result.Add(new TreeRowSepElement(indent));
                    continue;
                }

                if (dropdownItem.ChildCount() == 0)  // value node
                {
                    hasMeaningfulChild = true;
                    TreeRowValueElement valueElement = new TreeRowValueElement(dropdownItem.value, string.IsNullOrEmpty(dropdownItem.icon)? dropdownItem.displayName: $"<icon={dropdownItem.icon}/>{dropdownItem.displayName}", indent, _allowToggle);
                    if (curValues.Contains(dropdownItem.value))
                    {
                        valueElement.SetValueOn(true);
                        CurrentFocus ??= valueElement;
                    }

                    if (dropdownItem.disabled)
                    {
                        valueElement.SetEnabled(false);
                    }

                    object value = dropdownItem.value;
                    valueElement.OnClickedEvent.AddListener((on, isPrimary) => OnClickedEvent.Invoke(value, on, isPrimary));
                    result.Add(valueElement);

                    continue;
                }

                // (List<TreeViewItemData<IAdvancedDropdownList>> children, int resultId, bool childSelect) = MakeNestedItems(dropdownItem, curValues, incrId, selectedNestedIds, selectedValueIds);
                IReadOnlyList<TreeRowAbsElement> tailResult = MakeNestedTreeRow(indent + 1, dropdownItem, curValues);

                if (dropdownItem.ChildCount() > 0 && tailResult.Count == 0)
                {
                    continue;
                }

                hasMeaningfulChild = true;

                TreeRowFoldoutElement thisNode = new TreeRowFoldoutElement(dropdownItem.displayName, indent, true);
                foreach (TreeRowAbsElement childElement in tailResult)
                {
                    thisNode.AddContent(childElement);
                }

                result.Add(thisNode);
            }

            return hasMeaningfulChild ? result : Array.Empty<TreeRowAbsElement>();
        }
    }
}
#endif
