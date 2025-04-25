using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class IssueEA31 : MonoBehaviour
    {
        [Serializable]
        public class ScenarioStep {

            public enum StepType {
                Dialog = 1,
                Level = 2,
            }

            public StepType stepType;

            [GUIColor(EColor.Lime), ShowIf(nameof(stepType), StepType.Dialog)]
            public string dialog;
        }

        [SerializeField] private List<ScenarioStep> steps;
    }
}
