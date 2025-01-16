using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ButtonDynamicLabelExample : SaintsMonoBehaviour
    {
        [Serializable]
        public struct SaintsRow
        {
            public string dynamicLabel;

            public string DynamicLabelCallback => dynamicLabel;

            [Button("$" + nameof(DynamicLabelCallback))]
            private void ButtonWithDynamicLabel()
            {
            }
        }

        [SaintsRow] public SaintsRow row;

        public string dynamicLabel;

        [Button("$" + nameof(dynamicLabel))]
        private void ButtonWithDynamicLabel()
        {
        }

        [Button("Normal <icon=star.png/>Button Label")]
        private void ButtonWithNormalLabel()
        {
        }

        [Button]
        private void ButtonWithoutLabel()
        {
        }
    }
}
