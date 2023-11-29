using UnityEngine;

namespace SaintsField.Samples
{
    public class OverlayRichLabelExample: MonoBehaviour
    {
        [OverlayRichLabel("<color=grey>km/s")] public double speed = double.MinValue;
        [OverlayRichLabel("<icon=eye.png/>")] public string text;
        [OverlayRichLabel("<color=grey>/int", padding: 1)] public int count = int.MinValue;
        [OverlayRichLabel("<color=grey>/long", padding: 1)] public long longInt = long.MinValue;
        [OverlayRichLabel("<color=grey>suffix", end: true)] public string atEnd;
        [Space]
        [OverlayRichLabel("not ok", end: true)] public GameObject notSupported;
    }
}
