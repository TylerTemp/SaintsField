#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_DEBUG_UNITY_BROKEN_FALLBACK && !SAINTSFIELD_UI_TOOLKIT_DISABLE

using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.DropdownBase;
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

        private TreeRowAbsElement _currentFocus;
        private readonly bool _allowToggle;

        private readonly List<TreeRowAbsElement> flatList;

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

            _currentFocus = null;

            VisualElement treeContainer = new VisualElement
            {
                focusable = true,
            };
             flatList = new List<TreeRowAbsElement>();
            foreach (TreeRowAbsElement treeRow in treeRowElements)
            {
                treeContainer.Add(treeRow);
                foreach (TreeRowAbsElement rowAbsElement in FlatTreeRow(treeRow))
                {
                    flatList.Add(rowAbsElement);
                    switch (rowAbsElement)
                    {
                        case TreeRowValueElement tr:
                            tr.OnClickedEvent.AddListener((_, _) => _currentFocus = tr);
                            break;
                        case TreeRowFoldoutElement tf:
                            tf.RegisterValueChangedCallback(_ => _currentFocus = tf);
                            break;
                    }
                }
            }

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

            RegisterCallback<AttachToPanelEvent>(_ => toolbarSearchField.Q<TextField>().Q("unity-text-input").Focus());

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
                        if (_currentFocus is TreeRowFoldoutElement { value: true } foldoutElement)
                        {
                            foldoutElement.value = false;
                        }
                        else if(_currentFocus is { Parent: not null })
                        {
                            _currentFocus = _currentFocus.Parent;
                            // Debug.Log($"currentFocus={_currentFocus}");
                            foreach (TreeRowAbsElement treeRowAbsElement in flatList)
                            {
                                treeRowAbsElement.SetNavigateHighlight(_currentFocus == treeRowAbsElement);
                            }
                        }
                        return;
                    }
                    case NavigationMoveEvent.Direction.Right:
                    {
                        if (_currentFocus is TreeRowFoldoutElement { value: false } foldoutElement)
                        {
                            foldoutElement.value = true;
                        }
                        return;
                    }
                    default:
                        return;
                }

                TreeRowAbsElement toFocus = null;
                if (_currentFocus != null)
                {
                    List<TreeRowAbsElement> prevList = new List<TreeRowAbsElement>(flatList.Count);
                    for (int index = 0; index < flatList.Count; index++)
                    {
                        TreeRowAbsElement current = flatList[index];
                        if (current == _currentFocus)
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
                                    toFocus = flatList.LastOrDefault(each => each.Navigateable);
                                }

                                // Debug.Log($"up to {toFocus}");
                            }
                            else
                            {
                                toFocus = flatList.Skip(index + 1).FirstOrDefault(each => each.Navigateable)
                                          ?? flatList.FirstOrDefault(each => each.Navigateable);
                            }

                            break;
                        }

                        // Debug.Log($"{current} -> {currentFocus}");
                        prevList.Add(current);
                    }
                }

                if (_currentFocus == null)
                {
                    toFocus = isUp
                        ? flatList.LastOrDefault(each => each.Navigateable)
                        : flatList.FirstOrDefault(each => each.Navigateable);
                }

                if (toFocus != null)
                {
                    _currentFocus = toFocus;

                    // Debug.Log($"currentFocus={_currentFocus}");

                    foreach (TreeRowAbsElement treeRowAbsElement in flatList)
                    {
                        treeRowAbsElement.SetNavigateHighlight(toFocus == treeRowAbsElement);
                    }
                }
            }, TrickleDown.TrickleDown);
            RegisterCallback<KeyUpEvent>(e =>
            {

                if (_currentFocus is null)
                {
                    return;
                }

                // ReSharper disable once InvertIf
                if (e.keyCode is KeyCode.Space or KeyCode.Return or KeyCode.KeypadEnter)
                {
                    switch (_currentFocus)
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
            int result = 0;
            foreach (TreeRowAbsElement treeRowAbsElement in flatList)
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
                    TreeRowValueElement valueElement = new TreeRowValueElement(dropdownItem.displayName, indent, _allowToggle);
                    if (curValues.Contains(dropdownItem.value))
                    {
                        valueElement.SetValueOn(true);
                        _currentFocus ??= valueElement;
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
