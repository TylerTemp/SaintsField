#if UNITY_2021_3_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.UIToolkitElements;
using UnityEditor;
using UnityEngine.UIElements;

namespace SaintsField.Editor.ColorPalette.UIToolkit
{
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class CleanableLabelInputTypeAhead: CleanableTextInputTypeAhead
    {
        private bool _hoverOptions;
        private bool _focused;

        private readonly CleanableTextInput _cleanableTextInput;
        private readonly VisualElement _pop;

        public CleanableLabelInputTypeAhead(): this(null, null, null) { }

        private readonly SerializedProperty _colorInfoLabelsProp;
        private readonly ScrollView _root;
        private readonly SerializedProperty _colorInfoArray;


        public CleanableLabelInputTypeAhead(SerializedProperty colorInfoLabelsProp, ScrollView root,  SerializedProperty colorInfoArray): base(root)
        {
            _colorInfoLabelsProp = colorInfoLabelsProp;
            _root = root;
            _colorInfoArray = colorInfoArray;
        }

        protected override IEnumerable<string> GetOptions()
        {
            HashSet<string> curLabels = new HashSet<string>(GetLabels(_colorInfoLabelsProp))
            {
                "",  // filter out empty labels too
            };

            return Enumerable.Range(0, _colorInfoArray.arraySize)
                .Select(i => _colorInfoArray.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(ColorPaletteArray.ColorInfo.labels)))
                .SelectMany(GetLabels)
                .Except(curLabels)
                .Distinct();
        }

        private static IEnumerable<string> GetLabels(SerializedProperty colorInfoLabelsProp)
        {
            return Enumerable.Range(0, colorInfoLabelsProp.arraySize)
                .Select(i => colorInfoLabelsProp.GetArrayElementAtIndex(i).stringValue);
        }

        protected override bool OnInputOption(string value)
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
    }
}
#endif
