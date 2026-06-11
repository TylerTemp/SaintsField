#if !SAINTSFIELD_UI_TOOLKIT_DISABLE
using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using Spine;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Spine.SpinePathConstraintPickerDrawer
{
    public class SpinePathConstraintPickerElement: StringDropdownElement
    {
        private const string IconPath = "Spine/icon-constraintPath.png";

        public SpinePathConstraintPickerElement()
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

            foreach (PathConstraintData pathConstraintData in GetPathConstraintData(_skeletonData))
            {
                string pathConstraintName = pathConstraintData.Name;
                string iconName = $"<icon={IconPath}/>{pathConstraintName}";
                options.Add(iconName, pathConstraintName);
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

        private static IEnumerable<PathConstraintData> GetPathConstraintData(SkeletonData skeletonData)
        {
#if SAINTSFIELD_SPINE_UNITY_4_3_0_OR_NEWER
            return SpineUtils.GetConstraintData<PathConstraintData>(skeletonData);
#else
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (int i = 0; i < skeletonData.PathConstraints.Count; i++)
            {
                yield return skeletonData.PathConstraints.Items[i];
            }
#endif
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

            foreach (PathConstraintData pathConstraints in GetPathConstraintData(_skeletonData))
            {
                string pathConstraintsName = pathConstraints.Name;
                // ReSharper disable once InvertIf
                if (pathConstraintsName == value)
                {
                    UIToolkitUtils.SetLabel(Label, new []
                    {
                        new RichTextDrawer.RichTextChunk($"<icon={IconPath}/>", true, IconPath),
                        new RichTextDrawer.RichTextChunk(pathConstraintsName, false, pathConstraintsName),
                    }, _richTextDrawer);
                    tooltip = pathConstraintsName;
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

    public class SpinePathConstraintPickerField : BaseField<string>
    {
        public readonly SpinePathConstraintPickerElement SpinePathConstraintPickerElement;
        public SpinePathConstraintPickerField(string label, SpinePathConstraintPickerElement visualInput) : base(label, visualInput)
        {
            style.flexShrink = 1;
            visualInput.BindBound(this);
            SpinePathConstraintPickerElement = visualInput;
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            SpinePathConstraintPickerElement.SetValueWithoutNotify(newValue);
        }

        public override string value
        {
            get => SpinePathConstraintPickerElement.value;
            set => SpinePathConstraintPickerElement.value = value;
        }
    }
}

#endif
