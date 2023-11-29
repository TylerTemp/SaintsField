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
        [OverlayRichLabel(nameof(TakeAGuess), isCallback: true)] public int guess;
        [Space]
        [OverlayRichLabel("not ok", end: true)] public GameObject notSupported;

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
