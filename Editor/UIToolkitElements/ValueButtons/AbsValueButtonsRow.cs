using System.Collections.Generic;
using SaintsField.Editor.Core;
using SaintsField.Editor.Drawers.ValueButtonsDrawer;
using SaintsField.Editor.Linq;
using SaintsField.Editor.Utils;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.ValueButtons
{
    public abstract class AbsValueButtonsRow<T>: VisualElement where T: AbsValueButton
    {
        public readonly UnityEvent<object> OnButtonClicked = new UnityEvent<object>();

        protected AbsValueButtonsRow()
        {
            style.flexDirection = FlexDirection.Row;
        }

        // public IReadOnlyList<OptionButtonRawInfo> Buttons { get; private set; } = Array.Empty<OptionButtonRawInfo>();
        private readonly List<T> _buttons = new List<T>();

        protected abstract T MakeValueButton(IReadOnlyList<RichTextDrawer.RichTextChunk> chunks);

        public void ResetWithButtons(IEnumerable<ValueButtonRawInfo> buttons)
        {
            int processIndex = 0;
            foreach (ValueButtonRawInfo info in buttons)
            {
                T targetButton;
                if (_buttons.Count > processIndex)
                {
                    targetButton = _buttons[processIndex];
                    targetButton.ResetChunks(info.DisplayChunks);
                }
                else
                {
                    T newBtn = MakeValueButton(info.DisplayChunks);
                    newBtn.style.flexGrow = 1;
                    newBtn.style.flexShrink = 0;
                    newBtn.Value = info.Value;
                    // ValueButton newBtn = new ValueButton(info.DisplayChunks)
                    // {
                    //     style =
                    //     {
                    //         flexGrow = 1,
                    //         flexShrink = 0,
                    //     },
                    //     Value = info.Value,
                    // };
                    newBtn.SetEnabled(!info.Disabled);
                    Add(newBtn);
                    _buttons.Add(newBtn);
                    newBtn.clicked += () => OnButtonClicked.Invoke(newBtn.Value);
                    targetButton = newBtn;
                }
                targetButton.Value = info.Value;
                targetButton.SetEnabled(!info.Disabled);

                processIndex += 1;
            }

            // Debug.Log($"Sub processIndex = {processIndex} <- {_buttons.Count}");

            foreach (T toRemove in Util.ShrinkListTo(_buttons, processIndex))
            {
                toRemove.RemoveFromHierarchy();
            }

            foreach ((T button, int index)  in _buttons.WithIndex())
            {
                if (index == 0)
                {
                    button.style.borderLeftWidth = 1;
                    button.style.borderTopLeftRadius = button.style.borderBottomLeftRadius = 3;
                }

                button.style.borderRightWidth = 1;

                if (index == _buttons.Count - 1)
                {
                    button.style.borderTopRightRadius = button.style.borderBottomRightRadius = 3;
                }
            }
        }

        public void RefreshCurValue(object curValue)
        {
            foreach ((T valueButton, int index) in _buttons.WithIndex())
            {
                bool isFirst = index == 0;
                bool isLast = index == _buttons.Count - 1;
                valueButton.RefreshCurValue(curValue, isFirst, isLast);
            }
        }
    }
}
