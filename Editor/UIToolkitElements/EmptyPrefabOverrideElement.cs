#if UNITY_2021_3_OR_NEWER
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public class EmptyPrefabOverrideElement: VisualElement
    {
        private bool _overrideStyled;

        private readonly VisualElement _blueBar;

        public EmptyPrefabOverrideElement(SerializedProperty property)
        {
            this.TrackPropertyValue(property, p =>
            {
                OverrideStyle(p.prefabOverride);
            });

            _blueBar = new VisualElement
            {
                pickingMode = PickingMode.Ignore,
                style =
                {
                    position = Position.Absolute,
                    display = DisplayStyle.None,
                    width = 2,
                    // backgroundColor = Color.cyan,
                    backgroundColor = new Color(5/255f, 147/255f, 224/255f),
                    top = 1,
                    bottom = 1,
                    left = -15,
                },
            };

            Add(_blueBar);

            OverrideStyle(property.prefabOverride);
        }

        private void OverrideStyle(bool isOverride)
        {
            if (_overrideStyled == isOverride)
            {
                return;
            }

            if (isOverride)
            {
                AddToClassList(BindingExtensions.prefabOverrideUssClassName);
            }
            else
            {
                RemoveFromClassList(BindingExtensions.prefabOverrideUssClassName);
            }

            _overrideStyled = isOverride;
            _blueBar.style.display = isOverride ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
#endif
