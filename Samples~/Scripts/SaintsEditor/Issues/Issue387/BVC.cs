using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue387
{
    [Serializable]
    public class BVC
    {
        public SimData simData;

        [Button]
        public void CopyToTarget(Issue387Main tag)
        {
#if UNITY_EDITOR
            // BVCSimulationPanel.OpenNewWindow(this);
            tag.simDataInternal = simData;
#endif
        }
    }
}
