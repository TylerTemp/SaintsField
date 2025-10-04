#if UNITY_2021_3_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.AdvancedDropdownDrawer;
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

        private readonly Button _button;

        protected DropdownButtonGenElement(EnumMetaInfo metaInfo)
        {
            _metaInfo = metaInfo;

            TemplateContainer dropdownElement = UIToolkitUtils.CloneDropdownButtonTree();
            dropdownElement.style.flexGrow = 1;

            _button = dropdownElement.Q<Button>();

            _button.style.flexGrow = 1;

            _label = _button.Q<Label>();

            Add(dropdownElement);

            _button.clicked += ClickDropdown;
        }

        public void SetValueWithoutNotify(T newValue)
        {
            _hasCachedValue = true;
            _cachedValue = newValue;

            // Debug.Log(_metaInfo.IsFlags);
            object newEnum = Enum.ToObject(_metaInfo.EnumType, newValue);
            bool isInvalid = false;
            if(_metaInfo.IsFlags)
            {
                List<EnumMetaInfo.EnumValueInfo> renderLabels = new List<EnumMetaInfo.EnumValueInfo>();
                object zeroBit = Enum.ToObject(_metaInfo.EnumType, 0);
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
                        isInvalid = true;
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
                    if (metaInfoEnumValue.Value.Equals(newEnum))
                    {
                        SetLabelRichTextWithTooltips(_label, new[] { metaInfoEnumValue });
                        return;
                    }
                }
                SetLabelRichTextWithTooltips(_label, new[] { InvalidValueInfo(newValue) } );
                isInvalid = true;
            }

            if (isInvalid)
            {
                _button.tooltip = $"<color=red>Invalid Value</color> {newValue}";
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
                // Debug.Log($"event sent: {previous} -> {value}");
            }
        }

        private void ClickDropdown()
        {
            // Debug.Log($"bindingPath={bindingPath}; curValue={value}; has={_hasCachedValue}");
            // Debug.Log($"binding={binding}/{binding?.GetType()}. dataSource={dataSource}");

            bool isULong = _metaInfo.UnderType == typeof(ulong);
            #region MetaInfo

            AdvancedDropdownList<object> enumDropdown = new AdvancedDropdownList<object>("");
            List<object> curValues = new List<object>();
            object curValue = Enum.ToObject(_metaInfo.EnumType, value);
            bool containsEverythingOrNothing = false;

            object zeroEnum = Enum.ToObject(_metaInfo.EnumType, 0);
            if(_metaInfo.IsFlags)
            {
                if (_metaInfo.NothingValue.HasValue)
                {
                    enumDropdown.Add(_metaInfo.NothingValue.Label, _metaInfo.NothingValue.Value);
                }
                else
                {
                    enumDropdown.Add("Nothing", zeroEnum);
                }

                containsEverythingOrNothing = zeroEnum.Equals(curValue);

                if (containsEverythingOrNothing)
                {
                    curValues.Add(zeroEnum);
                }

                if (_metaInfo.EverythingValue.HasValue)
                {
                    enumDropdown.Add(_metaInfo.EverythingValue.Label, _metaInfo.EverythingValue.Value);
                }
                else
                {
                    enumDropdown.Add("Everything", _metaInfo.EverythingBit);
                }

                if (!containsEverythingOrNothing)
                {
                    containsEverythingOrNothing = EnumFlagsUtil.IsOnObject(curValue, _metaInfo.EverythingBit,
                        isULong);
                    if (containsEverythingOrNothing)
                    {
                        curValues.Add(_metaInfo.EverythingBit);
                    }
                }

                enumDropdown.AddSeparator();
            }

            foreach (EnumMetaInfo.EnumValueInfo enumInfo in _metaInfo.EnumValues)
            {
                // Debug.Log($"Add {enumInfo.Label} {enumInfo.Value}");
                enumDropdown.Add(enumInfo.Label, enumInfo.Value);
                if (!containsEverythingOrNothing)
                {
                    if (_metaInfo.IsFlags)
                    {
                        if (EnumFlagsUtil.IsOnObject(curValue, enumInfo.Value, isULong))
                        {
                            curValues.Add(enumInfo.Value);
                        }
                    }
                    else
                    {
                        if (enumInfo.Value.Equals(curValue))
                        {
                            curValues.Add(curValue);
                        }
                    }
                }
            }

            #endregion

            AdvancedDropdownMetaInfo metaInfo = new AdvancedDropdownMetaInfo
            {
                DropdownListValue = enumDropdown,
                CurValues = curValues,
                SelectStacks = Array.Empty<AdvancedDropdownAttributeDrawer.SelectStack>(),
                Error = "",
            };

            (Rect popWorldBound, float maxHeight) = SaintsAdvancedDropdownUIToolkit.GetProperPos(_button.worldBound);

            SaintsTreeDropdownUIToolkit sa = new SaintsTreeDropdownUIToolkit(
                metaInfo,
                _button.worldBound.width,
                maxHeight,
                false,
                (curItem, on) =>
                {
                    // Debug.Log($"curItem={curItem}");
                    // beforeSet?.Invoke(refDrawPayload.Value);
                    if (_metaInfo.IsFlags)
                    {

                        if (curItem.Equals(zeroEnum))
                        {
                            value = (T)Convert.ChangeType(0, _metaInfo.UnderType);
                        }
                        else if (curItem.Equals(_metaInfo.EverythingBit))
                        {
                            value = (T)Convert.ChangeType(_metaInfo.EverythingBit, _metaInfo.UnderType);
                        }
                        else if (on)
                        {
                            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                            if (_metaInfo.EverythingBit.Equals(curValue))
                            {
                                value = (T)Convert.ChangeType(curItem, _metaInfo.UnderType);
                            }
                            else
                            {
                                value = (T)Convert.ChangeType(EnumFlagsUtil.SetOnBitObject(curValue, curItem, isULong), _metaInfo.UnderType);
                            }
                        }
                        else
                        {
                            value = (T)Convert.ChangeType(EnumFlagsUtil.SetOffBitObject(curValue, curItem, isULong), _metaInfo.UnderType);
                        }
                    }
                    else
                    {
                        value = (T)Convert.ChangeType(curItem, _metaInfo.UnderType);
                    }
                    _button.Blur();
                    return null;
                }
            );
            UnityEditor.PopupWindow.Show(popWorldBound, sa);
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

            _button.tooltip = string.Join(", ", tooltips);
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
