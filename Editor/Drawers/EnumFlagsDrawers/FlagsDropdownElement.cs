#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.Linq;
using SaintsField.Editor.UIToolkitElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.EnumFlagsDrawers
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
            Label.Clear();
            Button.tooltip = "";
            long mask = _metaInfo.EnumType.GetEnumUnderlyingType() == typeof(uint)
                ? unchecked((uint)newValue)
                : newValue;

            if (newValue == 0)
            {
                EnumFlagsUtil.EnumDisplayInfo label;

                if (_metaInfo.BitValueToName.TryGetValue(newValue, out EnumFlagsUtil.EnumDisplayInfo displayInfo))
                {
                    label =  new EnumFlagsUtil.EnumDisplayInfo
                    {
                        HasRichName = true,
                        RichName = $"<b>{(displayInfo.HasRichName ? displayInfo.RichName : displayInfo.Name)}</b>"
                    };
                }
                else
                {
                    label = new EnumFlagsUtil.EnumDisplayInfo
                    {
                        HasRichName = true,
                        RichName = "<b>Nothing</b>",
                    };
                }

                // Label.text = label;
                AddLabelRichText(Label, label);
                return;
            }

            if((mask & _metaInfo.AllCheckedLong) == _metaInfo.AllCheckedLong)
            {
                EnumFlagsUtil.EnumDisplayInfo label;

                if (_metaInfo.BitValueToName.TryGetValue(mask, out EnumFlagsUtil.EnumDisplayInfo displayInfo))
                {
                    label = new EnumFlagsUtil.EnumDisplayInfo
                    {
                        HasRichName = true,
                        RichName = $"<b>{(displayInfo.HasRichName ? displayInfo.RichName : displayInfo.Name)}</b>",
                    };
                }
                else if (newValue == ~0 && _metaInfo.BitValueToName.TryGetValue(_metaInfo.AllCheckedLong, out EnumFlagsUtil.EnumDisplayInfo allBitsdisplayInfo))
                {
                    label = new EnumFlagsUtil.EnumDisplayInfo
                    {
                        HasRichName = true,
                        RichName = $"<b>{(allBitsdisplayInfo.HasRichName ? allBitsdisplayInfo.RichName : allBitsdisplayInfo.Name)}</b>",
                    };
                }
                else
                {
                    label = new EnumFlagsUtil.EnumDisplayInfo
                    {
                        HasRichName = true,
                        RichName = "<b>Everything</b>",
                    };
                }

                // Label.text = label;
                AddLabelRichText(Label, label);
                return;
            }

            List<long> selectedNameKeys = new List<long>();

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (long key in _metaInfo.BitValueToName.Keys)
            {
                if (key == 0)
                {
                    continue;
                }

                if ((mask & key) == key)
                {
                    selectedNameKeys.Add(key);
                }
            }

            if (selectedNameKeys.Count == 0)
            {
                AddLabelSingleText(Label, $"<color=red>?</color> {newValue}");
                Button.tooltip = "Invalid flags set";
                return;
            }

            // Remove any keys which are used to make up another key in the list
            // Up==1,Down==2,Vertical==3,Left==4 -> Vertical==3,Left==4
            // Up==1,Down==2,Vertical==3,Left==4,Right==8,Horizontal==12,All==15 -> All==15
            for (int i = selectedNameKeys.Count - 1; i >= 0; i--)
            {
                long key = selectedNameKeys[i];
                for (int j = 0; j < selectedNameKeys.Count; j++)
                {
                    long otherKey = selectedNameKeys[j];
                    if (otherKey != key && (otherKey & key) == key)
                    {
                        selectedNameKeys.RemoveAt(i);
                        break;
                    }
                }
            }

            int totalCount = selectedNameKeys.Count;
            foreach ((long keyIndex, int index) in selectedNameKeys.WithIndex())
            {
                if (!_metaInfo.BitValueToName.TryGetValue(keyIndex, out EnumFlagsUtil.EnumDisplayInfo displayInfo))
                {
                    continue;
                }

                // Debug.Log($"append {displayInfo.Name}:{displayInfo.RichName}");
                AddLabelRichText(Label, displayInfo);
                bool isLast = index == totalCount - 1;
                if (!isLast)
                {
                    AddLabelSingleText(Label, ", ");
                }
            }

        }

        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        private void AddLabelRichText(Label label, EnumFlagsUtil.EnumDisplayInfo displayInfo)
        {
            if (displayInfo.HasRichName)
            {
                // Debug.Log($"add rich {displayInfo.RichName}");
                foreach (VisualElement chunk in _richTextDrawer.DrawChunksUIToolKit(RichTextDrawer.ParseRichXmlWithProvider(displayInfo.RichName, new RichTextDrawer.EmptyRichTextTagProvider(displayInfo.Name))))
                {
                    label.Add(chunk);
                }

                Button.tooltip += displayInfo.Name;
            }
            else
            {
                // Debug.Log($"add text {displayInfo.Name}");
                AddLabelSingleText(label, displayInfo.Name);
            }
        }

        private void AddLabelSingleText(Label label, string content)
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

            Button.tooltip += content;
        }
    }
}
#endif
