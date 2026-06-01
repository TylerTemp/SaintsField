using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue395
{
    public class Issue395Main : SaintsMonoBehaviour
    {
        [Button]
        public void TakeScreenshot(ScreenshotCustomization customization = null)
        {
            Debug.Log(customization);
            Debug.Log(customization?.SpineMoments);
        }

    }
}
