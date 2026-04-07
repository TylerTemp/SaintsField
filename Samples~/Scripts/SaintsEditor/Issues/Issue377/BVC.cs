using System;
using SaintsField;
using SaintsField.Playa;

namespace Samples.Scripts.SaintsEditor.Issues.Issue377
{
    [Serializable]
    // ReSharper disable once InconsistentNaming
    public class BVC
    {
        [LayoutStart("fields", ELayout.Horizontal)]
        public float number;
        [PropRange(0f, 1f)]
        public float percent;

        public override string ToString()
        {
            return $"<BVC number={number} percent={percent} />";
        }
    }
}
