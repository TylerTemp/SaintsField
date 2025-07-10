using SaintsField.ComponentHeader;
using SaintsField.Samples.Scripts.SaintsEditor;

namespace SaintsField.Samples.Scripts.HeaderComponentExample.Issues
{
    // [HeaderLabel("$" + nameof(HeaderText))]
    public class Issue261Bold : SaintsMonoBehaviour
    {

        public bool isPlay;

        [HeaderLabel] public string BoldText => isPlay
            ? "<b>Playing</b>"
            : "<i>Not Playing</i>";

        [HeaderLabel]
        public string HeaderText =>
            isPlay
                // Unity build-in icon, see https://github.com/nukadelic/UnityEditorIcons
                ? "<icon=d_AudioListener Icon/>"
                // custom icon, search location see https://saintsfield.comes.today/general-attributes/label--text/richlabelnolabel
                : "<color=lime><icon=star.png/>";


    }
}
