using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue34 : MonoBehaviour
    {
        [SerializeReference, ReferencePicker] public TestData[] TestData;
    }

    [Serializable]
    public class TestData
    {
        [SerializeReference, ReferencePicker] public InternalTestData[] InternalTestData;
        [MinMaxSlider(0, 100)]
        public Vector2 TestMinMax = new Vector2(0, 2);
    }

    [Serializable]
    public class InternalTestData
    {
        [MinMaxSlider(0, 100)] public Vector2 InternalTestMinMax = new Vector2(0, 2);
    }
}
