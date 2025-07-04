#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.ColorPalette.UIToolkit
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class CleanableLabelInputTypeAhead: CleanableTextInputTypeAhead
    {
        private bool _hoverOptions;
        private bool _focused;

        private readonly CleanableTextInput _cleanableTextInput;
        // private readonly VisualElement _pop;

        // ReSharper disable once UnusedMember.Global
        public CleanableLabelInputTypeAhead(): this(null, null, null) { }

        private readonly SerializedProperty _colorInfoLabelsProp;
        private readonly SerializedProperty _colorInfoArray;


        public CleanableLabelInputTypeAhead(SerializedProperty colorInfoLabelsProp, ScrollView root, SerializedProperty colorInfoArray): base(root)
        {
            _colorInfoLabelsProp = colorInfoLabelsProp;
            _colorInfoArray = colorInfoArray;

            PopClosedEvent.AddListener(() => (root.contentContainer ?? root).style.minHeight = StyleKeyword.Null);
        }

        protected override IReadOnlyList<string> GetOptions()
        {
            HashSet<string> curLabels = new HashSet<string>(GetLabels(_colorInfoLabelsProp))
            {
                "",  // filter out empty labels too
            };
            string[] searchLowerFragments = CleanableTextInput.TextField.value.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string[] opts = Enumerable.Range(0, _colorInfoArray.arraySize)
                .Select(i => _colorInfoArray.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(ColorPaletteArray.ColorInfo.labels)))
                .SelectMany(GetLabels)
                .Except(curLabels)
                .Where(each => Search(searchLowerFragments, each))
                .Distinct()
                .ToArray();

            // _root.contentContainer.style.minHeight = opts.Length * SaintsPropertyDrawer.SingleLineHeight * 4;

            return opts;
        }

        private static IEnumerable<string> GetLabels(SerializedProperty colorInfoLabelsProp)
        {
            return Enumerable.Range(0, colorInfoLabelsProp.arraySize)
                .Select(i => colorInfoLabelsProp.GetArrayElementAtIndex(i).stringValue);
        }

        protected override bool OnInputOptionReturn(string value)
        {
            return OnInputOption(value);
        }

        protected override bool OnInputOptionTypeAhead(string value)
        {
            return OnInputOption(value);
        }

        private bool OnInputOption(string value)
        {
            if (GetLabels(_colorInfoLabelsProp).Any(each => each == value))
            {
                return false;
            }

            int index = _colorInfoLabelsProp.arraySize;
            _colorInfoLabelsProp.arraySize++;
            SerializedProperty newLabelProp = _colorInfoLabelsProp.GetArrayElementAtIndex(index);
            newLabelProp.stringValue = value;
            newLabelProp.serializedObject.ApplyModifiedProperties();
            return true;
        }

        protected override void PosTypeAhead(VisualElement root)
        {
            base.PosTypeAhead(root);
            VisualElement targetElement = root.contentContainer ?? root;

            // float popHeight;
            // if (double.IsNaN(Pop.resolvedStyle.height))
            // {
            //     popHeight = CurOptions.Count * SaintsPropertyDrawer.SingleLineHeight + 4;
            // }
            // else
            // {
            //     popHeight = Pop.resolvedStyle.height;
            // }

            targetElement.style.minHeight = Pop.worldBound.yMax;
        }
    }
}
#endif
