using System;
using System.Collections.Generic;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue237ReferencePicker : SaintsMonoBehaviour
    {
#if UNITY_2021_3_OR_NEWER
        public interface IReferenceData
        {
        }

        [Serializable]
        public struct Data1 : IReferenceData
        {
            public int Value1;
        }

        [SerializeReference, ReferencePicker] public List<IReferenceData> Data;
#endif
    }
}
