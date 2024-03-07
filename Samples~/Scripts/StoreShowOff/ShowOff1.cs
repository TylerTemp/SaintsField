using UnityEngine;

namespace SaintsField.Samples.Scripts.StoreShowOff
{
    public class ShowOff1 : MonoBehaviour
    {
        // well, Unity wants its store page to be fancy, otherwise it will not pass it...

        [Rate(1, 5)]
        [AboveRichLabel("Please rate our asset with some lovely <color=#ffff0033><icon=star.png/></color><color=#ffff0055><icon=star.png/></color><color=#ffff0088><icon=star.png/></color><color=#ffff00ff><icon=star.png/></color>")]
        [InfoBox(nameof(InfoBoxContent), show: nameof(InfoBoxShown), isCallback: true)]
        public int rating;

        public (EMessageType, string) InfoBoxContent()
        {
            if (rating == 1)
            {
                return (EMessageType.None, "Please let us know what we can do to improve our asset.");
            }
            if (rating >= 4)
            {
                return (EMessageType.Info, $"That's {rating} stars! Thank you for your support!");
            }

            return (EMessageType.None, "");
        }

        public bool InfoBoxShown() => rating == 1 || rating >= 4;
    }
}
