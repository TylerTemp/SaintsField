using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.UIToolkitElements;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_2021_3_OR_NEWER
namespace SaintsField.Editor.Drawers.EnumFlagsDrawers.FlagsDropdownDrawer
{
    public class FlagsDropdownElement: IntDropdownElement
    {
        private readonly EnumFlagsMetaInfo _metaInfo;

        public FlagsDropdownElement(EnumFlagsMetaInfo metaInfo)
        {
            _metaInfo = metaInfo;
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            CachedValue = newValue;

            if (newValue == 0)
            {
                // Label.text = "<b>Nothing</b>";
                Label.Clear();
                AddLabelSingleText(Label, "<b>Nothing</b>");
                return;
            }

            if((newValue & _metaInfo.AllCheckedInt) == _metaInfo.AllCheckedInt)
            {
                // Label.text = "<b>Everything</b>";
                Label.Clear();
                AddLabelSingleText(Label, "<b>Everything</b>");
                return;
            }

            List<EnumFlagsUtil.EnumDisplayInfo> selectedNames = new List<EnumFlagsUtil.EnumDisplayInfo>();

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (KeyValuePair<int, EnumFlagsUtil.EnumDisplayInfo> kvp in _metaInfo.BitValueToName)
            {
                if ((newValue & kvp.Key) == kvp.Key)
                {
                    selectedNames.Add(kvp.Value);
                }
            }

            if (selectedNames.Count == 0)
            {
                Label.Clear();
                AddLabelSingleText(Label, $"<color=red>?</color> {newValue}");
                return;
            }

            Label.Clear();
            int totalCount = selectedNames.Count;
            foreach ((EnumFlagsUtil.EnumDisplayInfo displayInfo, int index) in selectedNames.WithIndex())
            {
                AddLabelRichText(Label, displayInfo);
                bool isLast = index == totalCount - 1;
                if (!isLast)
                {
                    AddLabelSingleText(Label, ", ");
                }
            }

        }

        private RichTextDrawer _richTextDrawer;

        private void AddLabelRichText(Label label, EnumFlagsUtil.EnumDisplayInfo displayInfo)
        {
            if (displayInfo.HasRichName)
            {
                _richTextDrawer ??= new RichTextDrawer();
                VisualElement visualElement = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                    },
                };
                foreach (VisualElement chunk in _richTextDrawer.DrawChunksUIToolKit(RichTextDrawer.ParseRichXml(displayInfo.RichName, displayInfo.Name, null, null, null)))
                {
                    visualElement.Add(chunk);
                }
                label.Add(visualElement);
            }
            else
            {
                AddLabelSingleText(label, displayInfo.Name);
            }
        }

        private static void AddLabelSingleText(Label label, string content)
        {
            label.Add(new Label(content)
            {
                style =
                {
                    flexShrink = 0,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    paddingLeft = 0,
                    paddingRight = 0,
                    whiteSpace = WhiteSpace.Normal,
                },
                pickingMode = PickingMode.Ignore,
            });
        }
    }
}
#endif
