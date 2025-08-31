using SaintsField.ComponentHeader;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.HeaderComponentExample
{
    public class HeaderButtonExample : SaintsMonoBehaviour
    {
        [HeaderLeftButton]
        public void L1()
        {
            Debug.Log("L1");
        }

        [HeaderLeftButton("<color=brown><icon=star.png/>")]
        public void OnClickL2()
        {
            Debug.Log("OnClickL2");
        }

        [HeaderButton]
        public void R1()
        {
            Debug.Log("R1");
        }

        [HeaderButton("<color=lime><icon=star.png/></color>+1", "Add a star")]
        public void StartAdd()
        {
            rate = (rate + 1) % 6;
        }

        [HeaderButton("<color=gray><icon=star.png/></color>-1", "Remove a star")]
        public void StartRemove()
        {
            rate = (rate - 1 + 6) % 6;
            // Debug.Log("OnClickR2");
        }

        [Rate(1, 5)] public int rate;
    }
}
