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
            Label.Clear();
            Button.tooltip = "";

            if (newValue == 0)
            {
                string label = "<b>Nothing</b>";

                if (_metaInfo.BitValueToName.TryGetValue(newValue, out EnumFlagsUtil.EnumDisplayInfo displayInfo))
                    label = $"<b>{displayInfo.Name}</b>";

                // Label.text = label;
                AddLabelSingleText(Label, label);
                return;
            }

            if((newValue & _metaInfo.AllCheckedInt) == _metaInfo.AllCheckedInt)
            {
                string label = "<b>Everything</b>";

                if (_metaInfo.BitValueToName.TryGetValue(newValue, out EnumFlagsUtil.EnumDisplayInfo displayInfo))
                    label = $"<b>{displayInfo.Name}</b>";

                // Label.text = label;
                AddLabelSingleText(Label, label);
                return;
            }

            List<int> selectedNameKeys = new List<int>();

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (int key in _metaInfo.BitValueToName.Keys)
            {
                if (key == 0) continue;

                if ((newValue & key) == key)
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
                int key = selectedNameKeys[i];
                for (int j = 0; j < selectedNameKeys.Count; j++)
                {
                    int otherKey = selectedNameKeys[j];
                    if (otherKey != key && (otherKey & key) == key)
                    {
                        selectedNameKeys.RemoveAt(i);
                        break;
                    }
                }
            }

            int totalCount = selectedNameKeys.Count;
            foreach ((int keyIndex, int index) in selectedNameKeys.WithIndex())
            {
                if (!_metaInfo.BitValueToName.TryGetValue(keyIndex, out EnumFlagsUtil.EnumDisplayInfo displayInfo))
                    continue;

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
                foreach (VisualElement chunk in _richTextDrawer.DrawChunksUIToolKit(RichTextDrawer.ParseRichXml(displayInfo.RichName, displayInfo.Name, null, null, null)))
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
