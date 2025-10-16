#if UNITY_2021_3_OR_NEWER
using SaintsField.Editor.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public class BlueBar : VisualElement
    {
        public BlueBar()
        {
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.display = DisplayStyle.None;
            style.width = 2;
            // backgroundColor = Color.cyan,
            style.backgroundColor = new Color(5/255f, 147/255f, 224/255f);
            style.top = 1;
            style.bottom = 1;
            style.left = -15;
        }
    }

    public class EmptyPrefabOverrideElement: VisualElement
    {
        private bool _overrideStyled;

        private readonly VisualElement _blueBar;

        public EmptyPrefabOverrideElement(SerializedProperty property)
        {
            this.TrackPropertyValue(property, p =>
            {
                _overrideStyled = OverrideStyle(p, _overrideStyled, this, _blueBar);
            });

            _blueBar = new BlueBar();

            Add(_blueBar);

            _overrideStyled = OverrideStyle(property, _overrideStyled, this, _blueBar);
        }

        public static bool OverrideStyle(SerializedProperty property, bool currentOverride, VisualElement container, VisualElement blueBar)
        {
            bool isOverride = property.prefabOverride;
            if (currentOverride == isOverride)
            {
                return isOverride;
            }

            if (isOverride)
            {
                container.AddToClassList(BindingExtensions.prefabOverrideUssClassName);
            }
            else
            {
                container.RemoveFromClassList(BindingExtensions.prefabOverrideUssClassName);
            }

            blueBar.style.display = isOverride ? DisplayStyle.Flex : DisplayStyle.None;
            return isOverride;
        }
    }

    public class FoldoutPrefabOverrideElement: Foldout
    {
        private bool _overrideStyled;

        private readonly VisualElement _blueBar;

        public FoldoutPrefabOverrideElement(SerializedProperty property)
        {
            this.TrackPropertyValue(property, p =>
            {
                _overrideStyled = EmptyPrefabOverrideElement.OverrideStyle(p, _overrideStyled, this, _blueBar);
            });

            _blueBar = new BlueBar
            {
                style =
                {
                    height = SaintsPropertyDrawer.SingleLineHeight,
                },
            };

            hierarchy.Add(_blueBar);

            _overrideStyled = EmptyPrefabOverrideElement.OverrideStyle(property, _overrideStyled, this, _blueBar);
        }
    }
}
#endif
