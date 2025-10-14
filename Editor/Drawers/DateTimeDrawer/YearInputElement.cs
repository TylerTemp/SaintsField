#if UNITY_2021_3_OR_NEWER
using System;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;
#if !UNITY_2022_1_OR_NEWER
using UnityEditor.UIElements;
#endif

namespace SaintsField.Editor.Drawers.DateTimeDrawer
{
#if UNITY_6000_0_OR_NEWER && SAINTSFIELD_UI_TOOLKIT_XUML
    [UxmlElement]
#endif
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class YearInputElement: BindableElement, INotifyValueChanged<long>
    {
        public readonly IntegerField IntegerField;
        private long _cachedValue;

        public YearInputElement()
        {
            style.justifyContent = Justify.Center;
            IntegerField = new IntegerField
            {
                maxLength = 4,
                style =
                {
                    marginLeft = 0,
                    marginRight = 0,
                    // height = Length.Percent(100),
                    minWidth = 50,
                },
            };
            UIToolkitUtils.MakePlaceholderRight(IntegerField.Q<VisualElement>("unity-text-input"), DateTimeUtils.GetYearLabel());
            // .Add(new Label("y"));
            IntegerField.RegisterValueChangedCallback(evt => value = DateTimeUtils.WrapYear(_cachedValue, Mathf.Max(1, evt.newValue)));
            Add(IntegerField);
        }

        public void SetValueWithoutNotify(long newValue)
        {
            DateTime dt = new DateTime(newValue);
            int year = dt.Year;
            _cachedValue = newValue;
            IntegerField.SetValueWithoutNotify(year);
        }

        public long value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue == value)
                {
                    return;
                }

                long previous = this.value;
                SetValueWithoutNotify(value);

                using ChangeEvent<long> evt = ChangeEvent<long>.GetPooled(previous, value);
                evt.target = this;
                SendEvent(evt);
            }
        }
    }
}
#endif
