using System;

using UnityEngine;


namespace SaintsField.Samples.Scripts
{
    public class EndTextExample: MonoBehaviour
    {
        [EndText("<color=grey>km/s")] public float speed;
        [EndText("<icon=eye.png/>", padding: 0)] public Sprite eye;
        [EndText("$" + nameof(TakeAGuess))] public int guess;
        // [PostFieldRichLabel(nameof(Error), isCallback: true)] public GameObject errorCallback;

        [FieldReadOnly]
        [EndText("<icon=star.png/>", padding: 0)] public Sprite starDisabled;

        public string TakeAGuess()
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if(guess > 20)
            {
                return "<color=red>too high";
            }

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (guess < 10)
            {
                return "<color=blue>too low";
            }

            return "<color=green>acceptable!";
        }

        public bool show;

        [EndText("<color=gray>Dynamic", show: nameof(show))] public int num;
    }
}
