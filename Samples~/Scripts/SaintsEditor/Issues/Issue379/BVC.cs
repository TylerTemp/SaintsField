// using Generic;
// using Generic.Compare;
// using Systems.WorkplaceSystem.Resource;
using System;
using System.Collections.Generic;
using SaintsField.Playa;
using UnityEngine;

namespace SaintsField.Samples.Scripts.SaintsEditor.Issues.Issue379
{
    [Serializable]
    public class BVC
    {
        [Table] public List<Rule> rules;

        public ResourceType resourceType;

        public float CalculateValue(int input)
        {
            float result = startValue;

            // for (int i = 1; i <= input; i++)
            // {
            //     float currentPercentage = basePercentage;
            //     float addToResult = 0f;
            //
            //     foreach (var rule in rules)
            //     {
            //         if (rule.conditionsBasedInput.TrueForAll(x => x.TestCondition(i)))
            //         {
            //             switch (rule.effect.type)
            //             {
            //                 case RuleEffect.Type.SetPercentage:
            //                     currentPercentage = rule.effect.newPercentage;
            //                     break;
            //                 case RuleEffect.Type.AddToPercentage:
            //                     currentPercentage += rule.effect.addToPercentage;
            //                     break;
            //                 case RuleEffect.Type.AddPercentageOfPercentage:
            //                     currentPercentage +=
            //                         currentPercentage * (rule.effect.percentageOfPercentageToAdd / 100f);
            //                     break;
            //                 case RuleEffect.Type.SetResult:
            //                     return rule.effect.result;
            //                 case RuleEffect.Type.AddToResult:
            //                     addToResult += rule.effect.addToResult;
            //                     break;
            //                 default:
            //                     throw new ArgumentOutOfRangeException();
            //             }
            //         }
            //     }
            //
            //     // Apply percentage to result
            //     result += result.GetPercentage(currentPercentage) + addToResult;
            // }

            return result;
        }

        public float basePercentage;
        public float startValue;
        public SimData simData;

        [Serializable]
        public class Rule
        {
            [TableColumn("name")] public string name;
            [TableColumn("effect")] public RuleEffect effect;
            [TableColumn("conditions")] public List<BaseNumericalValueComparison> conditionsBasedInput;
        }


        [Button]
        public void ShowSimWindow()
        {
#if UNITY_EDITOR && !SAINTSFIELD_UI_TOOLKIT_DISABLE
          BVCSimulationPanel.OpenNewWindow(this);
#endif
        }

        [Serializable]
        public class SimData
        {
            public string displayName;
            public int InputRangeMin = 0;
            public int InputRangeMax = 100;

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
}
