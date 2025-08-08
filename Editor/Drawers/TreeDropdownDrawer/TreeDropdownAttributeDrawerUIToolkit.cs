using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.DropdownBase;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using SaintsField.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public partial class TreeDropdownAttributeDrawer
    {
        private static string NameButton(SerializedProperty property) => $"{property.propertyPath}__TreeDropdown_Button";
        private static string NameHelpBox(SerializedProperty property) => $"{property.propertyPath}__TreeDropdown_HelpBox";

        protected override VisualElement CreateFieldUIToolKit(SerializedProperty property,
            ISaintsAttribute saintsAttribute,
            IReadOnlyList<PropertyAttribute> allAttributes,
            VisualElement container,
            FieldInfo info,
            object parent)
        {
            // AdvancedDropdownMetaInfo initMetaInfo = GetMetaInfo(property, (AdvancedDropdownAttribute)saintsAttribute, info, parent, false);
            //
            // UIToolkitUtils.DropdownButtonField dropdownButton = UIToolkitUtils.MakeDropdownButtonUIToolkit(GetPreferredLabel(property));
            // dropdownButton.style.flexGrow = 1;
            // dropdownButton.name = NameButton(property);
            // dropdownButton.userData = initMetaInfo.CurValues;
            // dropdownButton.ButtonLabelElement.text = GetMetaStackDisplay(initMetaInfo);
            //
            // dropdownButton.AddToClassList(ClassAllowDisable);
            //
            // EmptyPrefabOverrideElement emptyPrefabOverrideElement = new EmptyPrefabOverrideElement(property);
            // emptyPrefabOverrideElement.Add(dropdownButton);
            //
            // return emptyPrefabOverrideElement;


            AdvancedDropdownMetaInfo metaInfo = AdvancedDropdownAttributeDrawer.GetMetaInfo(property, (PathedDropdownAttribute)saintsAttribute, info, parent, false);
            // Debug.Log(string.Join(",", metaInfo.CurValues));

            TreeView treeView = new TreeView();

            HashSet<int> selectedIds = new HashSet<int>();
            HashSet<int> selectedValueIds = new HashSet<int>();

            (List<TreeViewItemData<IAdvancedDropdownList>> nestedItems, int _, bool _) = MakeNestedItems(
                metaInfo.DropdownListValue,
                metaInfo.CurValues,
                0,
                selectedIds,
                selectedValueIds);

            // List<TreeViewItemData<string>> items = new List<TreeViewItemData<string>>(10);
            // for (int i = 0; i < 10; i++)
            // {
            //     int itemIndex = i * 10 + i;
            //
            //     List<TreeViewItemData<string>> treeViewSubItemsData = new List<TreeViewItemData<string>>(10);
            //     for (int j = 0; j < 10; j++)
            //     {
            //         treeViewSubItemsData.Add(new TreeViewItemData<string>(itemIndex + j + 1, $"Data {i + 1}-{j + 1}"));
            //     }
            //
            //     TreeViewItemData<string> treeViewItemData = new TreeViewItemData<string>(itemIndex, $"Data {i+1}", treeViewSubItemsData);
            //     items.Add(treeViewItemData);
            // }

            Func<VisualElement> makeItem = () => new Label();

            Action<VisualElement, int> bindItem = (e, i) =>
            {
                IAdvancedDropdownList item = treeView.GetItemDataForIndex<IAdvancedDropdownList>(i);
                int id = treeView.GetIdForIndex(i);
                ((Label)e).text = $"{(selectedIds.Contains(id) ? "✓" : "")}{item.displayName}({item.value})";
            };

            treeView.SetRootItems(nestedItems);
            treeView.makeItem = makeItem;
            treeView.bindItem = bindItem;
            // treeView.selectionType = SelectionType.Multiple;
            treeView.selectionType = SelectionType.Single;
            treeView.Rebuild();

            treeView.SetSelectionById(selectedIds);
            Debug.Log($"selectedIds={string.Join(",", selectedIds)}");

            // Callback invoked when the user double clicks an item
            // treeView.itemsChosen += (selectedItems) =>
            // {
            //     Debug.Log("Items chosen: " + string.Join(", ", selectedItems));
            // };

            // Callback invoked when the user changes the selection inside the TreeView
            treeView.selectedIndicesChanged += (selectedIndices) =>
            {
                // bool valueSelected = false;
                foreach (int index in selectedIndices)
                {
                    IAdvancedDropdownList r = treeView.GetItemDataForIndex<IAdvancedDropdownList>(index);
                    if (r.Count == 0)
                    {
                        // valueSelected = true;
                        break;
                    }
                }

                // if (!valueSelected)
                // {
                //     treeView.SetSelectionById(selectedIds);
                // }
            };

            return treeView;

        }

        private static (List<TreeViewItemData<IAdvancedDropdownList>> itemDatas, int resultId, bool hasSelect) MakeNestedItems(IAdvancedDropdownList dropdownLis,
            IReadOnlyList<object> curValues, int accId, HashSet<int> selectedNestedIds, HashSet<int> selectedValueIds)
        {
            List<TreeViewItemData<IAdvancedDropdownList>> result = new List<TreeViewItemData<IAdvancedDropdownList>>(dropdownLis.Count);

            bool hasSelect = false;
            int incrId = accId;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (IAdvancedDropdownList dropdownItem in dropdownLis)
            {
                incrId += 1;

                Debug.Log($"id={incrId} for {dropdownItem.displayName}");

                (List<TreeViewItemData<IAdvancedDropdownList>> children, int resultId, bool childSelect) = MakeNestedItems(dropdownItem, curValues, incrId, selectedNestedIds, selectedValueIds);

                result.Add(new TreeViewItemData<IAdvancedDropdownList>(
                    incrId,
                    dropdownItem,
                    children));

                bool selectedValue = (dropdownItem.Count == 0 && curValues.Contains(dropdownItem.value));
                if (childSelect || selectedValue)
                {
                    selectedNestedIds.Add(incrId);
                    hasSelect = true;
                    if (selectedValue)
                    {
                        selectedValueIds.Add(incrId);
                    }
                }

                incrId = resultId;
            }

            return (result, incrId, hasSelect);
        }
    }
}
