#if UNITY_2021_3_OR_NEWER
using UnityEngine;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Utils
{
#if UNITY_2021_3_OR_NEWER
    public static class UIToolkitUtils
    {

        public static void FixLabelWidthLoopUIToolkit(Label label)
        {

            FixLabelWidthUIToolkit(label);
            label.RegisterCallback<GeometryChangedEvent>(evt => FixLabelWidthUIToolkit((Label)evt.target));
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static void FixLabelWidthUIToolkit(Label label)
        {
            StyleLength autoLength = new StyleLength(StyleKeyword.Auto);
            StyleLength curLenght = label.style.width;
            float resolvedWidth = label.resolvedStyle.width;
            // if(curLenght.value != autoLength)
            // don't ask me why we need to compare with 0, ask Unity...
            if(!(curLenght.value.IsAuto() || curLenght.value == 0) && !float.IsNaN(resolvedWidth) && resolvedWidth > 0)
            {
                // Debug.Log($"try fix {label.style.width}({curLenght.value.IsAuto()}); {resolvedWidth > 0} {resolvedWidth}");
                label.style.width = autoLength;
                // label.schedule.Execute(() => label.style.width = autoLength);
            }
        }
    }
#endif
}
