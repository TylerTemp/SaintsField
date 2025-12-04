using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using Spine;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Spine.SpineIkConstraintPickerDrawer
{
    public class SpineIkConstraintPickerElement: StringDropdownElement
    {
        private const string IconPath = "Spine/icon-constraintIK.png";

        public SpineIkConstraintPickerElement()
        {
            Button.clicked += OnDropdownClick;
        }

        private void OnDropdownClick()
        {
            if (_skeletonData == null)
            {
                UIToolkitUtils.SetHelpBox(_helpBox, "No SkeletonData found");
                return;
            }

            AdvancedDropdownList<string> options = new AdvancedDropdownList<string>
            {
                {"[Empty String]", ""},
            };
            options.AddSeparator();

            for (int i = 0; i < _skeletonData.IkConstraints.Count; i++)
            {
                IkConstraintData ikConstraint = _skeletonData.IkConstraints.Items[i];
                string ikConstraintName = ikConstraint.Name;
                string iconName = $"<icon={IconPath}/>{ikConstraintName}";
                options.Add(iconName, ikConstraintName);
            }

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                DropdownListValue = options,
                CurValues = new[] { value },
            };

            (Rect wb, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos((_boundTarget ?? this).worldBound);
            SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                metaInfo,
                wb.width,
                maxHeight,
                false,
                (curItem, _) =>
                {
                    string curValue = (string)curItem;
                    value = curValue;
                    return null;
                }
            );

            // DebugPopupExample.SaintsAdvancedDropdownUIToolkit = sa;
            // var editorWindow = EditorWindow.GetWindow<DebugPopupExample>();
            // editorWindow.Show();

            UnityEditor.PopupWindow.Show(worldBound, sa);
        }

        private SkeletonData _skeletonData;

        public void BindSkeletonData(SkeletonData skeletonData)
        {
            if (_skeletonData == skeletonData)
            {
                return;
            }
            _skeletonData = skeletonData;
            RefreshDisplay();
        }

        private HelpBox _helpBox;

        public void BindHelpBox(HelpBox helpBox)
        {
            _helpBox = helpBox;
            RefreshDisplay();
        }

        private VisualElement _boundTarget;
        public void BindBound(VisualElement target) => _boundTarget = target;

        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        private void RefreshDisplay()
        {
            if (_skeletonData == null)
            {
                UIToolkitUtils.SetHelpBox(_helpBox, "No SkeletonData found");
                return;
            }

            UIToolkitUtils.SetHelpBox(_helpBox, "");

            if (string.IsNullOrEmpty(value))
            {
                Label.Clear();
                tooltip = "[Empty String]";
                return;
            }

            for (int i = 0; i < _skeletonData.IkConstraints.Count; i++)
            {
                IkConstraintData ikConstraint = _skeletonData.IkConstraints.Items[i];
                string ikConstraintName = ikConstraint.Name;
                if (ikConstraintName == value)
                {
                    UIToolkitUtils.SetLabel(Label, new []
                    {
                        new RichTextDrawer.RichTextChunk($"<icon={IconPath}/>", true, IconPath),
                        new RichTextDrawer.RichTextChunk(ikConstraintName, false, ikConstraintName),
                    }, _richTextDrawer);
                    tooltip = ikConstraintName;
                    return;
                }
            }

            UIToolkitUtils.SetLabel(Label, new []
            {
                new RichTextDrawer.RichTextChunk($"<color=red>?</color> {value}",false, $"<color=red>?</color> {value}"),
            }, _richTextDrawer);
            tooltip = $"Invalid: {value}";
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;
            RefreshDisplay();
        }
    }

    public class SpineIkConstraintPickerField : BaseField<string>
    {
        public readonly SpineIkConstraintPickerElement SpineIkConstraintPickerElement;
        public SpineIkConstraintPickerField(string label, SpineIkConstraintPickerElement visualInput) : base(label, visualInput)
        {
            style.flexShrink = 1;
            visualInput.BindBound(this);
            SpineIkConstraintPickerElement = visualInput;
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            SpineIkConstraintPickerElement.SetValueWithoutNotify(newValue);
        }

        public override string value
        {
            get => SpineIkConstraintPickerElement.value;
            set => SpineIkConstraintPickerElement.value = value;
        }
    }
}
