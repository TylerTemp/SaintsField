using System.Collections.Generic;
using SaintsField.Editor.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements.ValueButtons
{
    public abstract class AbsValueButton: Button
    {
        public IReadOnlyList<RichTextDrawer.RichTextChunk> Chunks;
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();
        private readonly Label _label;

        private object _value;

        public object Value
        {
            get => _value;
            set
            {
                _value = value;
                tooltip = $"{value}";
            }
        }

        protected AbsValueButton(IReadOnlyList<RichTextDrawer.RichTextChunk> chunks)
        {
            Chunks = chunks;
            style.marginLeft = 0;
            style.marginRight = 0;
            style.borderTopLeftRadius = style.borderTopRightRadius =
                style.borderBottomLeftRadius = style.borderBottomRightRadius = 0;
            style.borderLeftWidth = style.borderRightWidth = 0;

            _label = new Label
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                },
            };
            Add(_label);

            DrawChunks();
        }

        public void SetLabelCenter()
        {
            _label.style.justifyContent = Justify.Center;
        }

        public void ResetChunks(IReadOnlyList<RichTextDrawer.RichTextChunk> chunks)
        {
            Chunks = chunks;
            _label.Clear();
            DrawChunks();

        }

        public void RefreshCurValue(object curValue, bool isFirst, bool isLast)
        {
            if (IsOn(curValue))
            {
                SetOnStyle(isFirst, isLast);
            }
            else
            {
                SetOffStyle(isFirst, isLast);
            }
        }

        protected virtual void SetOnStyle(bool isFirst, bool isLast)
        {
            const float gray = 0.15f;
            const float grayBorder = 0.45f;
            style.backgroundColor = new Color(gray, gray, gray, 1f);
            Color borderColor = new Color(grayBorder, 0.6f, grayBorder, 1f);
            style.borderTopColor = style.borderBottomColor = borderColor;
            if (isFirst)
            {
                style.borderLeftColor = borderColor;
            }
            else if (isLast)
            {
                style.borderRightColor = borderColor;
            }

            style.borderLeftWidth = 1;
        }

        protected virtual void SetOffStyle(bool isFirst, bool isLast)
        {
            style.backgroundColor = StyleKeyword.Null;
            style.borderTopColor = style.borderBottomColor = style.borderLeftColor = style.borderRightColor = StyleKeyword.Null;
            if(!isFirst)
            {
                style.borderLeftWidth = 0;
            }
        }

        protected abstract bool IsOn(object curValue);

        private void DrawChunks()
        {
            foreach (VisualElement visualElement in _richTextDrawer.DrawChunksUIToolKit(Chunks))
            {
                _label.Add(visualElement);
            }
        }
    }
}
