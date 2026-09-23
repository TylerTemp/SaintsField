using SaintsField.Editor.Core;
using SaintsField.Editor.UIToolkitElements;
using SaintsField.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.Playa.Renderer.ChipsRenderer.ChipsInput
{
    public interface IDeletableChipDisplayResolver
    {
        (string nameWithPath, string icon, Color? color) GetDisplay(object value);
    }

    public class DeletableChip: DualButtonChip
    {
        public DeletableChip() : this(null)
        {
        }

        public DeletableChip(string label): base(label)
        {
            ChipRoot.style.borderTopLeftRadius =
                ChipRoot.style.borderTopRightRadius =
                    ChipRoot.style.borderBottomLeftRadius =
                        ChipRoot.style.borderBottomRightRadius =
                            5;

            Button1.style.display = DisplayStyle.None;
            Button1.pickingMode = PickingMode.Ignore;
            Button1.style.backgroundColor = new Color(1, 1, 1, 0.2f);
            Button2.clicked += OnDeleteButtonClicked;
        }

        public override void SetColor(Color color)
        {
            base.SetColor(color);
            Color iconColor = color;
            iconColor.a = 0.2f;
            Button1.style.backgroundColor = iconColor;
        }

        public override void UnsetColor()
        {
            base.UnsetColor();
            Button1.style.backgroundColor = new Color(1, 1, 1, 0.2f);
        }

        private SerializedProperty _arrayProp;
        private int _index = -1;
        private IDeletableChipDisplayResolver _displayResolver;
        private readonly RichTextDrawer _richTextDrawer = new RichTextDrawer();

        public void BindProp(SerializedProperty arrayProp, int index,
            IDeletableChipDisplayResolver displayResolver = null)
        {
            _arrayProp = arrayProp;
            _index = index;
            _displayResolver = displayResolver;

            SerializedProperty elemProp = arrayProp.GetArrayElementAtIndex(index);
            UIToolkitUtils.Unbind(this);
            this.TrackPropertyValue(elemProp, OnPropertyValueChanged);
            OnPropertyValueChanged(elemProp);
        }

        public void Rebind(int index)
        {
            BindProp(_arrayProp, index, _displayResolver);
        }

        public void OnDeleteButtonClicked()
        {
            // Debug.Log($"OnDeleteButtonClicked");
            if (_index != -1 && SerializedUtils.IsOk(_arrayProp))
            {
                _arrayProp.DeleteArrayElementAtIndex(_index);
                _arrayProp.serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnPropertyValueChanged(SerializedProperty targetProp)
        {
            (bool ok, object result) = SerializedUtils.GetPropertyValue(targetProp);
            // Debug.Log($"{ok}/{result}");
            if (ok)
            {
                if (_displayResolver == null)
                {
                    Label.text = $"{result}";
                }
                else
                {
                    (string display, string icon, Color? color) = _displayResolver.GetDisplay(result);
                    UIToolkitUtils.SetLabel(Label,
                        RichTextDrawer.ParseRichXmlWithProvider(display,
                            new RichTextDrawer.EmptyRichTextTagProvider()),
                        _richTextDrawer);
                    if (string.IsNullOrEmpty(icon))
                    {
                        UIToolkitUtils.SetDisplayStyle(Button1, DisplayStyle.None);
                    }
                    else
                    {
                        UIToolkitUtils.SetDisplayStyle(Button1, DisplayStyle.Flex);
                        Button1.style.backgroundImage = Util.LoadResource<Texture2D>(icon);
                        if (color == null)
                        {
                            UnsetColor();
                        }
                        else
                        {
                            SetColor(color.Value);
                        }
                    }
                }
            }
            else
            {
#if SAINTSFIELD_DEBUG
                Debug.LogWarning($"failed to obtain value from {targetProp.propertyPath}");
#endif
            }
        }
    }
}
