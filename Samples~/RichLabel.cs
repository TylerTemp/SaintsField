using UnityEngine;

namespace ExtInspector.Samples
{
    public class RichLabel: MonoBehaviour
    {
        [RichLabel("prefix:<color=red>some <color=\"green\"><b>[<color=yellow><icon='eye.png' /></color><label /></b>]</color>:su<color='yellow'> ff</color> ix</color> and long long long text")]
        public string richLabel;

        // public string GetRichLabel()
        // {
        //     return "<color=red>RichLabel</color>";
        // }
    }
}
