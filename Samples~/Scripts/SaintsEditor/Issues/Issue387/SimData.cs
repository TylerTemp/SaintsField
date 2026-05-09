using System;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue387
{
    [Serializable]
    public class SimData
    {
        public string displayName;

        [SerializeReference, ReferencePicker]
        public IAnyInterfece upgradeCostStackAsInput2;

        public bool ShouldShowUpgradeCostBalanceAsInput2()
        {
            var data = upgradeCostStackAsInput2 == null;
            // Debug.Log($"callback upgradeCostStackAsInput2 = {upgradeCostStackAsInput2} (is null: {data})");
            return data;
        }

        [ShowIf(nameof(ShouldShowUpgradeCostBalanceAsInput2))]
        // [ShowIf(nameof(upgradeCostStackAsInput2))]
        public bool test;


        public override string ToString()
        {
            return $"<SimData={displayName}/>";
        }
    }
}
