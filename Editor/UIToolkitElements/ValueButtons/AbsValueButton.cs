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
                tooltip = _obsolete? $"{value} <color=red>(Obsolete)</color>": $"{value}";
            }
        }

        private bool _obsolete;

        protected AbsValueButton(IReadOnlyList<RichTextDrawer.RichTextChunk> chunks, bool obsolete)
        {
            _obsolete = obsolete;

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

        public void ResetChunks(IReadOnlyList<RichTextDrawer.RichTextChunk> chunks, bool obsolete)
        {
            _obsolete = obsolete;
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
            if (_obsolete)
            {
                Color obsColor = EColor.OrangeRed.GetColor();
                obsColor.a = 0.6f;
                style.backgroundColor = obsColor;
                style.color = EColor.Gray.GetColor();
            }
            else
            {
                style.backgroundColor = new Color(gray, gray, gray, 1f);
                style.color = StyleKeyword.Null;
            }
            Color borderColor = new Color(grayBorder, 0.6f, grayBorder, 1f);
            style.borderTopColor = style.borderBottomColor = borderColor;

            StyleColor leftColor = StyleKeyword.Null;
            StyleColor rightColor = StyleKeyword.Null;
            // style.borderLeftColor = style.borderRightColor = StyleKeyword.Null;
            if (isFirst)
            {
                leftColor = borderColor;
                // style.borderLeftColor = borderColor;
            }
            else if (isLast)
            {
                rightColor = borderColor;
                // style.borderRightColor = borderColor;
            }

            style.borderLeftColor = leftColor;
            style.borderRightColor = rightColor;

            style.borderLeftWidth = 1;
        }

        protected virtual void SetOffStyle(bool isFirst, bool isLast)
        {
            if (_obsolete)
            {
                Color obsColor = EColor.Olive.GetColor();
                obsColor.a = 0.16f;
                style.backgroundColor = obsColor;
                style.color = EColor.Gray.GetColor();
            }
            else
            {
                style.backgroundColor = StyleKeyword.Null;
                style.color = StyleKeyword.Null;
            }
            style.borderTopColor = style.borderBottomColor = style.borderLeftColor = style.borderRightColor = StyleKeyword.Null;

            if (isFirst)
            {
                style.borderLeftWidth = StyleKeyword.Null;
            }
            else
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
