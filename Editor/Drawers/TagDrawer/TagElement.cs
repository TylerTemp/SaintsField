#if UNITY_2021_3_OR_NEWER
using System;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TagDrawer
{
    public class TagElement: StringDropdownElement
    {
        private VisualElement _boundElement;

        public void BindBound(VisualElement target) => _boundElement = target;

        public TagElement()
        {
            Button.clicked += MakeDropdown;
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (string tags in InternalEditorUtility.tags)
            {
                // ReSharper disable once InvertIf
                if (tags == newValue)
                {
                    SetLabelString(newValue);
                    return;
                }
            }

            SetLabelString(string.IsNullOrEmpty(newValue)
                ? ""
                : $"<color=red>?</color> ({newValue})");
        }

        private void MakeDropdown()
        {
            AdvancedDropdownList<string> dropdown = new AdvancedDropdownList<string>();
            dropdown.Add("[Empty String]", string.Empty);
            dropdown.AddSeparator();

            string selectedName = null;
            foreach (string tag in InternalEditorUtility.tags)
            {
                // dropdown.Add(path, (path, index));
                dropdown.Add(tag, tag);
                // ReSharper disable once InvertIf
                if (tag == value)
                {
                    selectedName = tag;
                }
            }

            dropdown.AddSeparator();
            dropdown.Add("Edit Tags...", null, false, "d_editicon.sml");

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                CurValues = selectedName is null ? Array.Empty<object>(): new object[] { selectedName },
                DropdownListValue = dropdown,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
            };

            VisualElement root = _boundElement ?? this;
            (Rect dropWorldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(root.worldBound);

            SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                metaInfo,
                root.worldBound.width,
                maxHeight,
                false,
                (curItem, _) =>
                {
                    string curValue = (string)curItem;
                    if (curValue == null)
                    {
                        Selection.activeObject =
                            AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset");
                        return null;
                    }

                    value = curValue;
                    return new[]{curValue};
                }
            );

            UnityEditor.PopupWindow.Show(dropWorldBound, sa);
        }
    }

    public class TagField : BaseField<string>
    {
        private readonly TagElement _tagElement;

        public TagField(string label, TagElement visualInput) : base(label, visualInput)
        {
            _tagElement = visualInput;
            visualInput.BindBound(this);
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            _tagElement.SetValueWithoutNotify(newValue);
        }

        public override string value
        {
            get => _tagElement.value;
            set => _tagElement.value = value;
        }
    }
}
#endif
