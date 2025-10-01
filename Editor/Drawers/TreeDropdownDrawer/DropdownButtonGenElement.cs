#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.EnumFlagsDrawers;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.TreeDropdownDrawer
{
    public class DropdownButtonGenElement<T>: BindableElement, INotifyValueChanged<T>
    {
        private readonly Label _label;

        private readonly EnumMetaInfo _metaInfo;

        private bool _hasCachedValue;
        private T _cachedValue;

        public readonly Button Button;

        protected DropdownButtonGenElement(EnumMetaInfo metaInfo)
        {
            _metaInfo = metaInfo;

            TemplateContainer dropdownElement = UIToolkitUtils.CloneDropdownButtonTree();
            dropdownElement.style.flexGrow = 1;

            Button = dropdownElement.Q<Button>();

            Button.style.flexGrow = 1;

            _label = Button.Q<Label>();

            Add(dropdownElement);
        }

        public void SetValueWithoutNotify(T newValue)
        {
            _hasCachedValue = true;
            _cachedValue = newValue;

            // Debug.Log(_metaInfo.IsFlags);
            if(_metaInfo.IsFlags)
            {
                List<EnumMetaInfo.EnumValueInfo> renderLabels = new List<EnumMetaInfo.EnumValueInfo>();
                object zeroBit = Enum.ToObject(_metaInfo.EnumType, 0);
                object newEnum = Enum.ToObject(_metaInfo.EnumType, newValue);
                if (newEnum.Equals(zeroBit))
                {
                    renderLabels.Add(_metaInfo.NothingValue.HasValue
                        ? _metaInfo.NothingValue
                        : new EnumMetaInfo.EnumValueInfo(zeroBit, "<b>Nothing</b>", "Nothing"));
                }
                else if (newEnum.Equals(_metaInfo.EverythingBit))
                {
                    renderLabels.Add(_metaInfo.EverythingValue.HasValue
                        ? _metaInfo.EverythingValue
                        : new EnumMetaInfo.EnumValueInfo(zeroBit, "<b>Everything</b>", "Everything"));
                }
                else
                {
                    bool isULong = _metaInfo.UnderType == typeof(ulong);

                    List<EnumMetaInfo.EnumValueInfo>
                        onValueInfos = new List<EnumMetaInfo.EnumValueInfo>(_metaInfo.EnumValues.Count);

                    foreach (EnumMetaInfo.EnumValueInfo otherEnumValue in _metaInfo.EnumValues)
                    {
                        // Debug.Log($"{newValue} - {otherEnumValue.Value} / {isULong}: {EnumFlagsUtil.IsOnObject(newValue, otherEnumValue.Value, isULong)}");
                        if (EnumFlagsUtil.IsOnObject(newEnum, otherEnumValue.Value, isULong))
                        {
                            // Debug.Log($"add {otherEnumValue.Label}");
                            onValueInfos.Add(otherEnumValue);
                        }
                    }

                    // remove sub on
                    List<int> removeIndices = new List<int>();
                    for (int index = 0; index < onValueInfos.Count; index++)
                    {
                        if (removeIndices.Contains(index))
                        {
                            continue;
                        }

                        object indexValue = onValueInfos[index].Value;
                        for (int checkingIndex = index + 1; checkingIndex < onValueInfos.Count; checkingIndex++)
                        {
                            if (removeIndices.Contains(checkingIndex))
                            {
                                continue;
                            }

                            object checkingValue = onValueInfos[checkingIndex].Value;

                            if (EnumFlagsUtil.IsOnObject(indexValue, checkingValue, isULong))
                            {
                                // Debug.Log($"removing {checkingIndex} as {indexValue} -> {checkingValue}");
                                removeIndices.Add(checkingIndex);
                                break;
                            }

                            if (EnumFlagsUtil.IsOnObject(checkingValue, indexValue, isULong))
                            {
                                // Debug.Log($"removing {index} as {checkingValue} -> {indexValue}");
                                removeIndices.Add(index);
                                break;
                            }
                        }
                    }

                    removeIndices.Sort((a, b) => b - a);
                    foreach (int removeIndex in removeIndices)
                    {
                        onValueInfos.RemoveAt(removeIndex);
                    }

                    if (onValueInfos.Count == 0)
                    {
                        renderLabels.Add(InvalidValueInfo(newValue));
                    }
                    else
                    {
                        renderLabels.AddRange(onValueInfos);
                    }
                }

                // Debug.Log($"rendering {string.Join(", ", renderLabels.Select(each => each.Label))}");

                SetLabelRichTextWithTooltips(_label, renderLabels);
            }
            else
            {
                foreach (EnumMetaInfo.EnumValueInfo metaInfoEnumValue in _metaInfo.EnumValues)
                {
                    if (metaInfoEnumValue.Value.Equals(newValue))
                    {
                        SetLabelRichTextWithTooltips(_label, new[] { metaInfoEnumValue });
                        return;
                    }
                }
                SetLabelRichTextWithTooltips(_label, new[] { InvalidValueInfo(newValue) } );
            }
        }

        private static EnumMetaInfo.EnumValueInfo InvalidValueInfo(T v) =>
            new EnumMetaInfo.EnumValueInfo(v, $"<color=red>?</color> {v}", "Invalid value");

        public T value
        {
            get => _hasCachedValue? _cachedValue: default;
            set
            {
                if (_hasCachedValue && _cachedValue.Equals(value))
                {
                    return;
                }

                T previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<T> evt = ChangeEvent<T>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }

        private RichTextDrawer _richTextDrawer;

        private void SetLabelRichTextWithTooltips(Label label, IReadOnlyList<EnumMetaInfo.EnumValueInfo> infos)
        {
            label.Clear();

            List<string> tooltips = new List<string>(infos.Count);

            for (int index = 0; index < infos.Count; index++)
            {
                EnumMetaInfo.EnumValueInfo info = infos[index];
                bool isLast = index == infos.Count - 1;
                if (info.OriginalLabel == info.Label) // normal label
                {
                    AddLabelSingleText(label, info.OriginalLabel);
                    tooltips.Add(info.OriginalLabel);
                }
                else
                {
                    _richTextDrawer ??= new RichTextDrawer();
                    // Debug.Log($"add rich {displayInfo.RichName}");
                    RichTextDrawer.RichTextChunk[] xmlNodes = RichTextDrawer
                        .ParseRichXml(info.Label, info.OriginalLabel, null, null, null).ToArray();
                    foreach (VisualElement chunk in _richTextDrawer.DrawChunksUIToolKit(xmlNodes))
                    {
                        label.Add(chunk);
                    }

                    tooltips.Add(string.Join("", xmlNodes.Where(each => !each.IsIcon).Select(each => each.Content)));
                }

                if (!isLast)
                {
                    AddLabelSingleText(label, ", ");
                }
            }

            Button.tooltip = string.Join(", ", tooltips);
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
