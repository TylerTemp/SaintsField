using UnityEngine;

namespace SaintsField.Samples.Scripts
{
    public class PostFieldRichLabelExample: MonoBehaviour
    {
        [EndText("<color=grey>km/s")] public float speed;
        [EndText("<icon=eye.png/>", padding: 0)] public Sprite eye;
        [EndText("$" + nameof(TakeAGuess))] public int guess;
        // [PostFieldRichLabel(nameof(Error), isCallback: true)] public GameObject errorCallback;

        [ReadOnly]
        [EndText("<icon=star.png/>", padding: 0)] public Sprite starDisabled;

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

        // public string Error()
        // {
        //     throw new Exception("Expected Exception");
        // }
    }
}
