using System.Collections;
using System.Linq;
using SaintsField.Playa;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class PlayaInfoShowExample : SaintsMonoBehaviour
    {
        public bool _complete;

        [PlayaBelowInfoBox("Upload Completed", show: nameof(_complete))]
        private IEnumerator UploadAndGetExcel()
        {
            _complete = false;
            foreach (int progress in Enumerable.Range(0, 100))
            {
                yield return null;
            }
            _complete = true;
        }
    }
}
