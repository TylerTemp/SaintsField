#if UNITY_2021_3_OR_NEWER
using System;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.Table
{
    public class TableCellFoldableElement: BindableElement, INotifyValueChanged<bool>
    {
        private class NoBubbleContainer : VisualElement, IBindable
        {
            public NoBubbleContainer()
            {
                RegisterCallback<ChangeEvent<bool>>(StopBoolChangeEvent);
            }

            private static void StopBoolChangeEvent(ChangeEvent<bool> evt)
            {
                evt.StopPropagation();
            }

            public IBinding binding { get; set; }
            public string bindingPath { get; set; }
        }

        public override VisualElement contentContainer { get; }

        private readonly Foldout _foldout;
        private readonly Button _expandDisplayButton;

        public TableCellFoldableElement()
        {
            style.paddingRight = 4;
            hierarchy.Add(_foldout = new Foldout
            {
                value = true,
                style =
                {
                    display = DisplayStyle.None,
                    flexGrow = 0,
                    flexShrink = 0,
                    width = 2,
                },
            });
            hierarchy.Add(contentContainer = new NoBubbleContainer
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                },
            });
            hierarchy.Add(_expandDisplayButton = new Button(() => value = !value)
            {
                text = "",
                style =
                {
                    display = DisplayStyle.None,
                    flexShrink = 1,
                    flexGrow = 1,
                    overflow = Overflow.Hidden,
                    textOverflow = TextOverflow.Ellipsis,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    borderLeftWidth = 0,
                    borderTopWidth = 0,
                    borderRightWidth = 0,
                    borderBottomWidth = 0,
                    backgroundColor = StyleKeyword.Initial,
                    marginTop = 0,
                    marginBottom = 0,
                    marginLeft = 0,
                    marginRight = 0,
                    paddingLeft = 1,
                    paddingRight = 1,
                },
            });
            style.flexDirection = FlexDirection.Row;
        }

        private Func<string> _getPreviewText;

        public void BindButton(Func<string> getPreviewText)
        {
            _getPreviewText = getPreviewText;
            if (!value)
            {
                _expandDisplayButton.text = _getPreviewText.Invoke();
            }
        }

        public void ToggleDisplay(bool on)
        {
            UIToolkitUtils.SetDisplayStyle(_foldout, on? DisplayStyle.Flex: DisplayStyle.None);
        }

        private string _viewKeyId;

        public void SetViewKey(string id)
        {
            _viewKeyId = id;
            bool newValue = SessionState.GetBool(id, true);
            FoldoutAndHeightUpdate(newValue);
        }

        public void SetValueWithoutNotify(bool newValue)
        {
            FoldoutAndHeightUpdate(newValue);
            if (!string.IsNullOrEmpty(_viewKeyId))
            {
                SessionState.SetBool(_viewKeyId, newValue);
            }
        }

        private void FoldoutAndHeightUpdate(bool newValue)
        {
            _foldout.SetValueWithoutNotify(newValue);
            style.maxHeight = newValue ? StyleKeyword.Initial : EditorGUIUtility.singleLineHeight + 2;
            if (newValue)
            {
                UIToolkitUtils.SetDisplayStyle(contentContainer, DisplayStyle.Flex);
                UIToolkitUtils.SetDisplayStyle(_expandDisplayButton, DisplayStyle.None);
            }
            else
            {
                UIToolkitUtils.SetDisplayStyle(contentContainer, DisplayStyle.None);
                UIToolkitUtils.SetDisplayStyle(_expandDisplayButton, DisplayStyle.Flex);

                string buttonText = _getPreviewText?.Invoke() ?? "";
                if (_expandDisplayButton.text != buttonText)
                {
                    _expandDisplayButton.text = buttonText;
                }
            }
        }

        public bool value
        {
            get => _foldout.value;
            set => _foldout.value = value;
        }
    }
}
#endif
