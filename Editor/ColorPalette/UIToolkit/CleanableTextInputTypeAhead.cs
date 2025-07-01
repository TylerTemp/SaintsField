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
        public CleanableTextInputTypeAhead(SerializedProperty colorInfoLabelsProp, ScrollView root,  SerializedProperty colorInfoArray)
        {
            VisualElement pop = CreatePop();

            CleanableTextInput input = new CleanableTextInput();
            input.TextField.RegisterCallback<FocusInEvent>(_ =>
            {
                Vector2 worldAnchor = new Vector2(input.worldBound.xMin, input.worldBound.yMax);
                Vector2 localAnchor = root.contentContainer.WorldToLocal(worldAnchor);
                pop.style.top = localAnchor.y;
                pop.style.left = localAnchor.x;

                FillOptions(input.TextField.value, pop, colorInfoLabelsProp, colorInfoArray);

                root.Add(pop);
            });
            input.TextField.RegisterCallback<BlurEvent>(_ => root.Remove(pop));

            input.TextField.RegisterValueChangedCallback(e => FillOptions(e.newValue, pop, colorInfoLabelsProp, colorInfoArray));
            Add(input);
        }

        private static void FillOptions(string search, VisualElement pop, SerializedProperty colorInfoLabelsProp,
            SerializedProperty colorInfoArray)
        {
            string[] searchLowerPieces = search.ToLower().Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            HashSet<string> curLabels = Enumerable.Range(0, colorInfoLabelsProp.arraySize)
                .Select(i => colorInfoLabelsProp.GetArrayElementAtIndex(i).stringValue)
                .ToHashSet();
            IEnumerable<string> options = Enumerable.Range(0, colorInfoArray.arraySize)
                .Select(i => colorInfoArray.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(ColorPaletteArray.ColorInfo.labels)))
                .SelectMany(labelsProp =>
                    Enumerable.Range(0, labelsProp.arraySize)
                    .Select(labelIndex => labelsProp.GetArrayElementAtIndex(labelIndex).stringValue))
                .Except(curLabels)
                .OrderBy(each => each.ToLower())
                .Distinct()
                .Where(label => Search(searchLowerPieces, label));

            pop.Clear();
            HashSet<string> alreadyOptions = new HashSet<string>();
            foreach (string option in options)
            {
                if(alreadyOptions.Add(option))
                {
                    pop.Add(new ButtonItem(option));
                }
            }
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
