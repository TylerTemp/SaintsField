#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_DEBUG_UNITY_BROKEN_FALLBACK && !SAINTSFIELD_UI_TOOLKIT_DISABLE

using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.DropdownBase;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
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

        public SaintsTreeDropdownElement(AdvancedDropdownMetaInfo metaInfo)
        {
            // VisualElement root = new VisualElement();

            CleanableTextInputFullWidth cleanableTextInput = new CleanableTextInputFullWidth(null);
            Add(cleanableTextInput);

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

            IEnumerable<VisualElement> treeRowElements = MakeNestedTreeRow(0, Array.Empty<ListSearchToken>(),
                metaInfo.DropdownListValue,
                curValues);

            VisualElement treeContainer = new VisualElement();
            foreach (VisualElement treeRow in treeRowElements)
            {
                treeContainer.Add(treeRow);
            }
            Add(treeContainer);

            cleanableTextInput.TextField.RegisterValueChangedCallback(evt =>
            {
                string searchText = evt.newValue;

                ListSearchToken[] searchTokens = string.IsNullOrWhiteSpace(searchText)
                    ? Array.Empty<ListSearchToken>()
                    : SerializedUtils.ParseSearch(searchText).ToArray();

                treeContainer.Clear();
                foreach (TreeRowAbsElement treeRowAbsElement in MakeNestedTreeRow(0, searchTokens,
                             metaInfo.DropdownListValue,
                             curValues))
                {
                    treeContainer.Add(treeRowAbsElement);
                }
            });
        }

        private IReadOnlyList<TreeRowAbsElement> MakeNestedTreeRow(int indent, IReadOnlyList<ListSearchToken> searchTokens, IAdvancedDropdownList dropdownLis, ICollection<object> curValues)
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

                bool isSearchMatched;
                if (searchTokens.Count == 0)
                {
                    isSearchMatched = true;
                }
                else if (dropdownItem.isSeparator)
                {
                    isSearchMatched = searchTokens.Count == 0;
                }
                else if (dropdownItem.ChildCount() > 0)  // leaf node is always matched, unless the children are empty
                {
                    isSearchMatched = true;
                }
                else
                {
                    isSearchMatched = IsSearchMatched(dropdownItem.absolutePathFragments, searchTokens);
                    // Debug.Log($"matched for {string.Join('/', dropdownItem.absolutePathFragments)}");
                }

                if (!isSearchMatched)
                {
                    continue;
                }

                if (dropdownItem.ChildCount() == 0)  // value node
                {
                    hasMeaningfulChild = true;
                    TreeRowValueElement valueElement = new TreeRowValueElement(dropdownItem.displayName, indent, true);
                    if (curValues.Contains(dropdownItem.value))
                    {
                        valueElement.SetValueOn(true);
                    }

                    object value = dropdownItem.value;
                    valueElement.OnClickedEvent.AddListener((on, isPrimary) => OnClickedEvent.Invoke(value, on, isPrimary));
                    result.Add(valueElement);

                    continue;
                }

                // (List<TreeViewItemData<IAdvancedDropdownList>> children, int resultId, bool childSelect) = MakeNestedItems(dropdownItem, curValues, incrId, selectedNestedIds, selectedValueIds);
                IReadOnlyList<TreeRowAbsElement> tailResult = MakeNestedTreeRow(indent + 1, searchTokens, dropdownItem, curValues);

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

        private static bool IsSearchMatched(IReadOnlyList<string> sourceFragments, IReadOnlyList<ListSearchToken> searchTokens)
        {
            IReadOnlyList<string> sourceLow = sourceFragments.Select(each => each.ToLower()).ToArray();
            foreach (ListSearchToken token in searchTokens)
            {
                string tokenLow = token.Token.ToLower();

                bool hasMatched = false;
                foreach (string sourceContent in sourceLow)
                {
                    if (token.Type == ListSearchType.Exclude && sourceContent.Contains(tokenLow))
                    {
                        return false;
                    }

                    if (token.Type == ListSearchType.Include && sourceContent.Contains(tokenLow))
                    {
                        hasMatched = true;
                        break;
                    }
                }

                if (!hasMatched)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
#endif
