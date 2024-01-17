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
            if(label.style.width != StyleKeyword.Auto)
            {
                label.style.width = StyleKeyword.Auto;
            }
        }
    }
#endif
}
