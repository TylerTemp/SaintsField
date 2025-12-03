using System;
using System.Collections.Generic;
using System.Linq;
using SaintsField.Editor.Core;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Drawers.ValueButtonsDrawer
{
    public abstract class AbsValueButtonsArrangeElement<T>: VisualElement where T: AbsValueButton
    {
        private readonly AbsValueButtonsCalcElement<T> _valueButtonsCalcElement;
        private readonly AbsValueButtonsRow<T> _mainRow;
        private readonly List<AbsValueButtonsRow<T>> _subRows = new List<AbsValueButtonsRow<T>>();
        public readonly UnityEvent<object> OnButtonClicked = new UnityEvent<object>();

        protected abstract AbsValueButtonsRow<T> MakeValueButtonsRow();

        public AbsValueButtonsArrangeElement(AbsValueButtonsCalcElement<T> valueButtonsCalcElement, AbsValueButtonsRow<T> mainRow)
        {
            style.position = Position.Relative;

            valueButtonsCalcElement.style.position = Position.Absolute;
            valueButtonsCalcElement.style.top = 0;
            valueButtonsCalcElement.style.left = 0;

            Add(_valueButtonsCalcElement = valueButtonsCalcElement);
            // ReSharper disable once VirtualMemberCallInConstructor
            Add(_mainRow = mainRow);
            _mainRow.OnButtonClicked.AddListener(OnButtonClicked.Invoke);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
            _valueButtonsCalcElement.ReadyEvent.AddListener(OnCalcReadyEvent);
            RegisterCallback<AttachToPanelEvent>(_ => CheckWidth());
        }

        private float _selfWidth = -1;
        private float _subWidth = -1;

        private void OnGeometryChangedEvent(GeometryChangedEvent _)
        {
            // Debug.Log("OnGeometryChangedEvent");
            // if (SubContainer != null && SubContainer.style.display == DisplayStyle.None)
            // {
            //     return;
            // }

            CheckWidth();
        }

        private bool CheckWidth()
        {
            bool changed = false;
            float resolvedWidth = resolvedStyle.width;
            if (!double.IsNaN(resolvedWidth) && resolvedWidth > 0)
            {
                float useWidth = Mathf.Max(1, resolvedWidth - 18);  // remove the button space
                if (Math.Abs(_selfWidth - useWidth) > float.Epsilon)
                {
                    changed = true;
                    _selfWidth = useWidth;
                }
            }

            if (_subContainer != null)
            {
                var subResolvedWidth = _subContainer.resolvedStyle.width;
                if (!double.IsNaN(subResolvedWidth) && subResolvedWidth > 0)
                {
                    if (Math.Abs(_subWidth - subResolvedWidth) > float.Epsilon)
                    {
                        changed = true;
                        _subWidth = subResolvedWidth;
                    }
                }
            }
            else
            {
                // Debug.Log("No subcontainer set, skip");
                return false;
            }

            if (_selfWidth > 0 && _subWidth > 0 && changed)
            {
                // Debug.Log($"width changed {_selfWidth}, {_subWidth}, CheckArrange");
                CheckArrange();
                return true;
            }

            if (_selfWidth > 0 && _subWidth > 0 && !_pending)
            {
                // Debug.Log($"width not pending check range, CheckArrange");
                CheckArrange();
                return true;
            }

            // Debug.Log($"no width changed {_selfWidth}, {_subWidth}(null={SubContainer == null}), _pending={_pending}");
            return false;
        }

        private VisualElement _subContainer;

        public void BindSubContainer(VisualElement target)
        {
            _subContainer = target;
            _subContainer.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
            // Debug.Log("BindSubContainer, CheckWidth");
            CheckWidth();
            // Debug.Log("BindSubContainer, CheckArrange");
            // CheckArrange();
        }

        private IReadOnlyList<ValueButtonRawInfo> _curRawOptions;

        public void UpdateButtons(IReadOnlyList<ValueButtonRawInfo> options)
        {
            _curRawOptions = options;
            // Debug.Log($"UpdateButtons {options.Count}, CheckArrange");
            if (!CheckWidth())
            {
                CheckArrange();
            }
        }

        private bool _pending;

        private void CheckArrange()
        {
            if (_subContainer == null)
            {
                // Debug.Log("No SubContainer, skip");
                return;
            }

            if (_curRawOptions == null)
            {
                // Debug.Log("No curRawOptions, skip");
                return;
            }

            if (_selfWidth < 0 || _subWidth < 0)
            {
                // Debug.Log("No width, skip");
                return;
            }

            _pending = true;
            // Debug.Log($"Start to calc {curRawOptions.Count}, set pending={_pending}");
            _valueButtonsCalcElement.SetButtonLabels(_curRawOptions.Select(each => each.DisplayChunks));
        }

        public readonly UnityEvent<bool> OnCalcArrangeDone = new UnityEvent<bool>();

        private void OnCalcReadyEvent(IReadOnlyList<(IReadOnlyList<RichTextDrawer.RichTextChunk>, float)> results)
        {
            // Debug.Log("OnCalcReadyEvent");
            if (CheckWidth())  // width changed, skip and wait for next call
            {
                // Debug.Log("OnCalcReadyEvent width changed, skip and wait for next call");
                return;
            }

            _pending = false;

            if (_curRawOptions == null || results.Count != _curRawOptions.Count)
            {
                // Debug.Log("No curRawOptions || count mismatch, skip");
                return;  // wait for the next calc
            }

            if (_selfWidth < 0 || _subWidth < 0)
            {
                // Debug.Log("No width, skip");
                return;
            }

            int rowIndex = 0;
            float accWidth = _selfWidth;
            List<List<ValueButtonRawInfo>> splitRowInfos = new List<List<ValueButtonRawInfo>>();

            for (int index = 0; index < results.Count; index++)
            {
                (IReadOnlyList<RichTextDrawer.RichTextChunk> resultChunks, float resultWidth) = results[index];
                ValueButtonRawInfo valueButtonRawInfo = _curRawOptions[index];
                if (!resultChunks.SequenceEqual(valueButtonRawInfo.DisplayChunks))
                {
                    return;  // mismatch, must be changed during rendering. Wait next call
                }

                // is it a new row?
                if (splitRowInfos.Count <= rowIndex)
                {
                    List<ValueButtonRawInfo> newRow = new List<ValueButtonRawInfo>
                    {
                        valueButtonRawInfo,
                    };
                    splitRowInfos.Add(newRow);
                    accWidth = (index == 0? _selfWidth: _subWidth) - resultWidth;
                    // Debug.Log($"On {rowIndex} add new row with {resultWidth}, left width = {accWidth}");
                    if (accWidth < 0)
                    {
                        // Debug.Log($"On {rowIndex} pump to next row");
                        rowIndex += 1;
                    }
                }
                else  // old row
                {
                    if (accWidth < resultWidth)  // no enough space, move to next row
                    {
                        List<ValueButtonRawInfo> newRow = new List<ValueButtonRawInfo>
                        {
                            valueButtonRawInfo,
                        };
                        splitRowInfos.Add(newRow);
                        accWidth = _subWidth - resultWidth;
                        // Debug.Log($"On {rowIndex} pump to new row with {resultWidth}, left width = {accWidth}");
                        rowIndex += 1;
                    }
                    else  // add item to current row
                    {
                        splitRowInfos[rowIndex].Add(valueButtonRawInfo);
                        accWidth -= resultWidth;
                        // Debug.Log($"On {rowIndex} now count={splitRowInfos[rowIndex].Count} reduce {resultWidth}, left width {accWidth}");
                    }
                }
            }

            int processedIndex = 0;
            // bool hasSubRows = false;
            for (int index = 0; index < splitRowInfos.Count; index++)
            {
                processedIndex = index;
                List<ValueButtonRawInfo> rowInfos = splitRowInfos[index];
                AbsValueButtonsRow<T> useRow;
                if (index == 0)
                {
                    useRow = _mainRow;
                }
                else
                {
                    int subRowIndex = index - 1;
                    if (_subRows.Count <= subRowIndex)  // need to add new rows
                    {
                        AbsValueButtonsRow<T> newRow = MakeValueButtonsRow();
                        newRow.OnButtonClicked.AddListener(OnButtonClicked.Invoke);
                        _subContainer.Add(newRow);
                        _subRows.Add(newRow);
                        useRow = newRow;
                    }
                    else
                    {
                        useRow = _subRows[subRowIndex];
                    }
                }

                // Debug.Log($"Set row {index} count {rowInfos.Count}({string.Join(", ", rowInfos.Select(each => each.Value))})");
                useRow.ResetWithButtons(rowInfos);
            }

            // Debug.Log($"processedIndex={processedIndex}, _subRows={_subRows.Count}");

            foreach (AbsValueButtonsRow<T> removed in Util.ShrinkListTo(_subRows, processedIndex))
            {
                removed.RemoveFromHierarchy();
            }

            OnCalcArrangeDone.Invoke(splitRowInfos.Count > 1);

            // if (processedIndex < _subRows.Count)
            // {
            //     for (int toRemoveIndex = _subRows.Count - 1; toRemoveIndex >= processedIndex; toRemoveIndex--)
            //     {
            //         // Debug.Log($"processedIndex remove index {toRemoveIndex}");
            //         OptionButtonsRow ele = _subRows[toRemoveIndex];
            //         ele.RemoveFromHierarchy();
            //         _subRows.RemoveAt(toRemoveIndex);
            //     }
            // }
        }

        public void RefreshCurValue(object curValue)
        {
            foreach (AbsValueButtonsRow<T> valueButtonsRow in _subRows.Append(_mainRow))
            {
                valueButtonsRow.RefreshCurValue(curValue);
            }
        }
    }
}
