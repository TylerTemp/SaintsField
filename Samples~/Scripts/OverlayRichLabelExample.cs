using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class OverlayRichLabelExample: MonoBehaviour
    {
        [OverlayText("<color=grey>km/s")] public double speed = double.MinValue;
        [OverlayText("<icon=eye.png/>")] public string text;
        [OverlayText("<color=grey>/int")] public int count = int.MinValue;
        [OverlayText("<color=grey>/long")] public long longInt = long.MinValue;
        [OverlayText("<color=grey>suffix", end: true)] public string atEnd;
        [OverlayText("$" + nameof(TakeAGuess))] public int guess;
        [Space]
        [OverlayText("not ok", end: true)] public GameObject notSupported;

        [ReadOnly]
        [OverlayText("<icon=eye.png/>", padding: 1)] public string textDisabled;

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
