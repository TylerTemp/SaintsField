using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue34 : MonoBehaviour
    {
#if UNITY_2021_3_OR_NEWER
        [SerializeReference, ReferencePicker] public TestData[] TestData;
#endif
    }

    [Serializable]
    public class TestData
    {
#if UNITY_2021_3_OR_NEWER
        [SerializeReference, ReferencePicker] public InternalTestData[] InternalTestData;
#endif
        [MinMaxSlider(0, 100)]
        public Vector2 TestMinMax = new Vector2(0, 2);
    }

    [Serializable]
    public class InternalTestData
    {
        [MinMaxSlider(0, 100)] public Vector2 InternalTestMinMax = new Vector2(0, 2);
    }
}
