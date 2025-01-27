using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting
{
    public class WrongOrder : MonoBehaviour
    {
        [SerializeField, SepTitle(EColor.Aqua), Range(0f, 1f), InfoBox("No info box")]
        public float defaultRange;

        [SerializeField, ListDrawerSettings] public int[] requiresSaints;
    }
}
