using System.Drawing;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
using SaintsField.Editor.Drawers.TreeDropdownDrawer;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using Spine;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.Spine.SpineBonePickerDrawer
{
    public class SpineBonePickerElement: StringDropdownElement
    {
        private const string IconPath = "Spine/icon-bone.png";

        public SpineBonePickerElement()
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

            AdvancedDropdownList<string> options = new AdvancedDropdownList<string>();
            for (int i = 0; i < _skeletonData.Bones.Count; i++)
            {
                BoneData bone = _skeletonData.Bones.Items[i];
                string boneName = bone.Name;
                // jointName = "root/hip/bone" to show a hierarchial tree.
                string jointName = $"<icon={IconPath}/>{boneName}";
                BoneData iterator = bone;
                while ((iterator = iterator.Parent) != null)
                {
                    jointName = $"{iterator.Name}/{jointName}";
                }
                options.Add(jointName, boneName);
            }

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                DropdownListValue = options,
                CurValues = new string[] { value },
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

            for (int i = 0; i < _skeletonData.Bones.Count; i++)
            {
                BoneData bone = _skeletonData.Bones.Items[i];
                string boneName = bone.Name;
                // // jointName = "root/hip/bone" to show a hierarchial tree.
                // string jointName = boneName;
                // BoneData iterator = bone;
                // while ((iterator = iterator.Parent) != null)
                // {
                //     jointName = $"{iterator.Name}/{jointName}";
                // }

                if (boneName == value)
                {
                    UIToolkitUtils.SetLabel(Label, new []
                    {
                        new RichTextDrawer.RichTextChunk($"<icon={IconPath}/>", true, IconPath),
                        new RichTextDrawer.RichTextChunk(boneName, false, boneName),
                    }, _richTextDrawer);
                    tooltip = boneName;
                    return;
                }
            }

            UIToolkitUtils.SetLabel(Label, new []
            {
                new RichTextDrawer.RichTextChunk($"<color=red>?</color> {value}",false, $"<color=red>?</color> {value}"),
            }, _richTextDrawer);
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            CachedValue = newValue;
            RefreshDisplay();
        }
    }

    public class SpineBonePickerField : BaseField<string>
    {
        public readonly SpineBonePickerElement SpineBonePickerElement;
        public SpineBonePickerField(string label, SpineBonePickerElement visualInput) : base(label, visualInput)
        {
            style.flexShrink = 1;
            visualInput.BindBound(this);
            SpineBonePickerElement = visualInput;
        }

        public override void SetValueWithoutNotify(string newValue)
        {
            SpineBonePickerElement.SetValueWithoutNotify(newValue);
        }

        public override string value
        {
            get => SpineBonePickerElement.value;
            set => SpineBonePickerElement.value = value;
        }
    }
}
