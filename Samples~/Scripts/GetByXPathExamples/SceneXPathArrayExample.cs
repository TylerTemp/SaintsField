using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.GetByXPathExamples
{
    public class SceneXPathArrayExample : SaintsMonoBehaviour
    {
        [SerializeField, GetByXPath("scene:://OfferingSkillInUseBlocker")]
        private GameObject[] _offeringSkillInUseBlockers;

        [SerializeField, ListDrawerSettings, GetByXPath("scene:://OfferingSkillInUseBlocker")]
        private GameObject[] _blockersWithListSettings;

        [SerializeField, GetByXPath("scene:://OfferingSkillInUseBlocker")]
        private GameObject[] _initiallyEmptyBlockers;
    }
}
