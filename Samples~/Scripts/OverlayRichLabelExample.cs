using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class OverlayRichLabelExample: MonoBehaviour
    {
        [OverlayRichLabel("<color=grey>km/s")] public double speed = double.MinValue;
        [OverlayRichLabel("<icon=eye.png/>")] public string text;
        [OverlayRichLabel("<color=grey>/int")] public int count = int.MinValue;
        [OverlayRichLabel("<color=grey>/long")] public long longInt = long.MinValue;
        [OverlayRichLabel("<color=grey>suffix", end: true)] public string atEnd;
        [OverlayRichLabel("$" + nameof(TakeAGuess))] public int guess;
        [Space]
        [OverlayRichLabel("not ok", end: true)] public GameObject notSupported;

        [ReadOnly]
        [OverlayRichLabel("<icon=eye.png/>", padding: 1)] public string textDisabled;

        public string TakeAGuess()
        {
            if(guess > 20)
            {
                return "<color=red>too high";
            }

            if (guess < 10)
            {
                return "<color=blue>too low";
            }

            return "<color=green>acceptable!";
        }
    }
}
