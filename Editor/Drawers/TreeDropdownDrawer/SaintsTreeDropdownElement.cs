#if UNITY_2022_2_OR_NEWER && !SAINTSFIELD_DEBUG_UNITY_BROKEN_FALLBACK && !SAINTSFIELD_UI_TOOLKIT_DISABLE

using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.DropdownBase;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public class SaintsTreeDropdownElement: VisualElement
    {
        public UnityEvent<object> OnValueSelected { get; } = new UnityEvent<object>();
        public UnityEvent OnMeaningfulInteraction { get; } = new UnityEvent();

        public SaintsTreeDropdownElement(AdvancedDropdownMetaInfo metaInfo)
        {
            // VisualElement root = new VisualElement();

            CleanableTextInputFullWidth cleanableTextInput = new CleanableTextInputFullWidth(null);
            Add(cleanableTextInput);

            IEnumerable<VisualElement> treeRowElements = MakeNestedTreeRow(0, Array.Empty<ListSearchToken>(),
                metaInfo.DropdownListValue,
                metaInfo.CurValues);

            VisualElement treeContainer = new VisualElement();
            foreach (VisualElement treeRow in treeRowElements)
            {
                treeContainer.Add(treeRow);
            }
            Add(treeContainer);

            // TreeView treeView = new TreeView();
            // Add(treeView);
            //
            // HashSet<int> selectedIds = new HashSet<int>();
            // HashSet<int> selectedValueIds = new HashSet<int>();
            //
            // List<TreeViewItemData<IAdvancedDropdownList>> nestedItems = MakeNestedItems(
            //     Array.Empty<ListSearchToken>(),
            //     metaInfo.DropdownListValue,
            //     metaInfo.CurValues,
            //     0,
            //     selectedIds,
            //     selectedValueIds).ItemDatas;
            //
            // // List<TreeViewItemData<string>> items = new List<TreeViewItemData<string>>(10);
            // // for (int i = 0; i < 10; i++)
            // // {
            // //     int itemIndex = i * 10 + i;
            // //
            // //     List<TreeViewItemData<string>> treeViewSubItemsData = new List<TreeViewItemData<string>>(10);
            // //     for (int j = 0; j < 10; j++)
            // //     {
            // //         treeViewSubItemsData.Add(new TreeViewItemData<string>(itemIndex + j + 1, $"Data {i + 1}-{j + 1}"));
            // //     }
            // //
            // //     TreeViewItemData<string> treeViewItemData = new TreeViewItemData<string>(itemIndex, $"Data {i+1}", treeViewSubItemsData);
            // //     items.Add(treeViewItemData);
            // // }
            //
            // Func<VisualElement> makeItem = () => new Label();
            //
            // Action<VisualElement, int> bindItem = (e, i) =>
            // {
            //     IAdvancedDropdownList item = treeView.GetItemDataForIndex<IAdvancedDropdownList>(i);
            //     int id = treeView.GetIdForIndex(i);
            //     ((Label)e).text = $"{(selectedIds.Contains(id) ? "✓" : "")}{item.displayName}({item.value})";
            // };
            //
            // treeView.SetRootItems(nestedItems);
            // treeView.makeItem = makeItem;
            // treeView.bindItem = bindItem;
            // treeView.selectionType = SelectionType.Multiple;
            // treeView.selectionType = SelectionType.Single;
            // treeView.Rebuild();
            //
            // HashSet<int> initSelectedIds = new HashSet<int>(selectedIds);
            // HashSet<int> initSelectedValueIds = new HashSet<int>(selectedValueIds);
            //
            // treeView.SetSelectionById(initSelectedIds);
            // Debug.Log($"selectedIds={string.Join(",", initSelectedIds)}");
            //
            // cleanableTextInput.TextField.RegisterValueChangedCallback(evt =>
            // {
            //     string searchText = evt.newValue.Trim();
            //     selectedIds.Clear();
            //     selectedValueIds.Clear();
            //
            //     if(string.IsNullOrEmpty(searchText))
            //     {
            //         selectedIds.UnionWith(initSelectedIds);
            //         selectedValueIds.UnionWith(initSelectedValueIds);
            //         treeView.SetRootItems(nestedItems);
            //         treeView.Rebuild();
            //         return;
            //     }
            //
            //     Debug.Log($"searchText={searchText}");
            //     treeView.SetRootItems(MakeNestedItems(
            //         SerializedUtils.ParseSearch(searchText).ToArray(),
            //         metaInfo.DropdownListValue,
            //         metaInfo.CurValues,
            //         0,
            //         selectedIds,
            //         selectedValueIds).ItemDatas);
            //     treeView.Rebuild();
            // });
            //
            // // Callback invoked when the user double clicks an item
            // // treeView.itemsChosen += (selectedItems) =>
            // // {
            // //     Debug.Log("Items chosen: " + string.Join(", ", selectedItems));
            // // };
            //
            // double lastClickTime = double.MinValue;
            // double lastSelectTime = double.MinValue;
            // IReadOnlyList<object> selectedItems = null;
            // HashSet<object> initSelectedItems = new HashSet<object>(initSelectedIds.Select(each => treeView.GetItemDataForId<IAdvancedDropdownList>(each).value));
            //
            // // Callback invoked when the user changes the selection inside the TreeView
            // treeView.selectedIndicesChanged += selectedIndices =>
            // {
            //     bool valueSelected = false;
            //     List<object> curSelectedItems = new List<object>();
            //     foreach (int index in selectedIndices)
            //     {
            //         IAdvancedDropdownList r = treeView.GetItemDataForIndex<IAdvancedDropdownList>(index);
            //         // ReSharper disable once InvertIf
            //         if (r.Count == 0)
            //         {
            //             valueSelected = true;
            //             curSelectedItems.Add(r.value);
            //         }
            //     }
            //
            //     if (!valueSelected)
            //     {
            //         selectedItems = null;
            //         lastSelectTime = double.MinValue;
            //         return;
            //     }
            //
            //     // Debug.Log($"{string.Join(", ", selectedItems)}");
            //     selectedItems = curSelectedItems;
            //     lastSelectTime = EditorApplication.timeSinceStartup;
            //     CheckClickTarget();
            // };
            //
            // treeView.RegisterCallback<MouseDownEvent>(_ =>
            // {
            //     lastClickTime = EditorApplication.timeSinceStartup;
            //     CheckClickTarget();
            // });
            //
            // treeView.RegisterCallback<KeyUpEvent>(e =>
            // {
            //     // Debug.Log(e.keyCode);
            //     if (e.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
            //     {
            //         if(selectedItems != null)
            //         {
            //             Debug.Log($"enter {string.Join(", ", selectedItems)}");
            //             CheckSelect();
            //         }
            //         OnMeaningfulInteraction.Invoke();
            //     }
            // }, TrickleDown.TrickleDown);
            return;

            // void CheckClickTarget()
            // {
            //     double diff = lastSelectTime - lastClickTime;
            //     // Debug.Log(diff);
            //     if (diff is > -0.01d and < 0.01d && selectedItems != null)
            //     {
            //         Debug.Log($"click {string.Join(", ", selectedItems)}");
            //         CheckSelect();
            //         OnMeaningfulInteraction.Invoke();
            //     }
            // }
            //
            // void CheckSelect()
            // {
            //     foreach (object selectedItem in selectedItems)
            //     {
            //         if (!initSelectedItems.Contains(selectedItem))
            //         {
            //             Debug.Log($"select {selectedItem}");
            //             OnValueSelected.Invoke(selectedItem);
            //             return;
            //         }
            //     }
            // }
        }

        private static IReadOnlyList<TreeRowAbsElement> MakeNestedTreeRow(int indent, IReadOnlyList<ListSearchToken> searchTokens, IAdvancedDropdownList dropdownLis, IReadOnlyList<object> curValues)
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
                    TreeRowValueElement valueElement = new TreeRowValueElement(dropdownItem.displayName, indent, true);
                    if (curValues.Contains(dropdownItem.value))
                    {
                        valueElement.SetValueOn(true);
                    }
                    result.Add(valueElement);

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
                    Debug.Log($"matched for {string.Join('/', dropdownItem.absolutePathFragments)}");
                }

                if (!isSearchMatched)
                {
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

                // bool selectedValue = (dropdownItem.Count == 0 && curValues.Contains(dropdownItem.value));
                // if (tailResult.HasSelect || selectedValue)
                // {
                //     selectedNestedIds.Add(incrId);
                //     hasSelect = true;
                //     if (selectedValue)
                //     {
                //         selectedValueIds.Add(incrId);
                //     }
                // }

                // incrId = tailResult.ResultId;
                // isEmptyNode = false;
            }

            return hasMeaningfulChild ? result : Array.Empty<TreeRowAbsElement>();
        }

        // public readonly struct MakeNestedItemResult
        // {
        //     public readonly List<TreeViewItemData<IAdvancedDropdownList>> ItemDatas;
        //     public readonly int ResultId;
        //     public readonly bool HasSelect;
        //     // public readonly bool IsEmptyNode;
        //
        //     public MakeNestedItemResult(List<TreeViewItemData<IAdvancedDropdownList>> itemDatas, int resultId, bool hasSelect)
        //     {
        //         ItemDatas = itemDatas;
        //         ResultId = resultId;
        //         HasSelect = hasSelect;
        //         // IsEmptyNode = isEmptyNode;
        //     }
        // }

        // private static MakeNestedItemResult MakeNestedItems(IReadOnlyList<ListSearchToken> searchTokens, IAdvancedDropdownList dropdownLis,
        //     IReadOnlyList<object> curValues, int accId, HashSet<int> selectedNestedIds, HashSet<int> selectedValueIds)
        // {
        //     List<TreeViewItemData<IAdvancedDropdownList>> result = new List<TreeViewItemData<IAdvancedDropdownList>>(dropdownLis.Count);
        //
        //     bool hasSelect = false;
        //     int incrId = accId;
        //     // bool isEmptyNode = true;
        //     // ReSharper disable once LoopCanBeConvertedToQuery
        //     foreach (IAdvancedDropdownList dropdownItem in dropdownLis)
        //     {
        //         bool isSearchMatched;
        //         if (searchTokens.Count == 0)
        //         {
        //             isSearchMatched = true;
        //         }
        //         else if (dropdownItem.isSeparator)
        //         {
        //             isSearchMatched = searchTokens.Count == 0;
        //         }
        //         else if (dropdownItem.ChildCount() > 0)  // leaf node is always matched, unless the children are empty
        //         {
        //             isSearchMatched = true;
        //         }
        //         else
        //         {
        //             isSearchMatched = IsSearchMatched(dropdownItem.absolutePathFragments, searchTokens);
        //             Debug.Log($"matched for {string.Join('/', dropdownItem.absolutePathFragments)}");
        //         }
        //
        //         if (!isSearchMatched)
        //         {
        //             continue;
        //         }
        //
        //         incrId += 1;
        //
        //         Debug.Log($"id={incrId} for {dropdownItem.displayName}");
        //
        //         // (List<TreeViewItemData<IAdvancedDropdownList>> children, int resultId, bool childSelect) = MakeNestedItems(dropdownItem, curValues, incrId, selectedNestedIds, selectedValueIds);
        //         MakeNestedItemResult tailResult = MakeNestedItems(searchTokens, dropdownItem, curValues, incrId, selectedNestedIds, selectedValueIds);
        //
        //         if (dropdownItem.ChildCount() > 0 && tailResult.ItemDatas.Count == 0)
        //         {
        //             continue;
        //         }
        //
        //         result.Add(new TreeViewItemData<IAdvancedDropdownList>(
        //             incrId,
        //             dropdownItem,
        //             tailResult.ItemDatas));
        //
        //         bool selectedValue = (dropdownItem.Count == 0 && curValues.Contains(dropdownItem.value));
        //         if (tailResult.HasSelect || selectedValue)
        //         {
        //             selectedNestedIds.Add(incrId);
        //             hasSelect = true;
        //             if (selectedValue)
        //             {
        //                 selectedValueIds.Add(incrId);
        //             }
        //         }
        //
        //         incrId = tailResult.ResultId;
        //         // isEmptyNode = false;
        //     }
        //
        //     return new MakeNestedItemResult(result, incrId, hasSelect);
        // }

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
