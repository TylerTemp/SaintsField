using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue387
{
    public class Issue387Main : SaintsMonoBehaviour
    {
        // [DefaultExpand]
        // public SimData editingSimData;

        // [GetInChildren] public Issue387CopyFrom cf;

        public SimData simDataInternal;

        // [Button]
        // private void CopyFrom()
        // {
        //     editingSimData = GetComponentInChildren<Issue387CopyFrom>().simData;
        // }

        [ShowInInspector] private string curName => $"{simDataInternal?.upgradeCostStackAsInput2}";
    }
}
