using SaintsField.ComponentHeader;
using SaintsField.Samples.Scripts.SaintsEditor;

namespace SaintsField.Samples.Scripts.HeaderComponentExample
{
    public class HeaderGhostButtonExample: SaintsMonoBehaviour
    {

        [HeaderGhostLeftButton("<icon=pencil.png/>")]
        public void Edit()
        {
        }

        [HeaderGhostButton("<icon=refresh.png/>", "Play")]
        public void Play()
        {
        }

        [HeaderGhostButton("<color=gray><icon=save.png/>", "Pause")]
        public void Pause()
        {
        }

        [HeaderGhostButton("<color=gray><icon=trash.png/>", "Resume")]
        public void Resume()
        {
        }
    }
}
