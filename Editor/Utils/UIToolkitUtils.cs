#if UNITY_2021_3_OR_NEWER
using UnityEngine;
using UnityEngine.UIElements;
#endif

namespace SaintsField.Editor.Utils
{
    public static class UIToolkitUtils
    {
        public static void FixLabelWidthLoopUIToolkit(Label label)
        {
#if UNITY_2021_3_OR_NEWER
            FixLabelWidthUIToolkit(label);
            label.RegisterCallback<GeometryChangedEvent>(evt => FixLabelWidthUIToolkit((Label)evt.target));
#endif
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static void FixLabelWidthUIToolkit(Label label)
        {
#if UNITY_2021_3_OR_NEWER
            if(label.style.width != StyleKeyword.Auto)
            {
                label.style.width = StyleKeyword.Auto;
            }
#endif
        }
    }
}
