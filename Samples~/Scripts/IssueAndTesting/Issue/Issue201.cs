using System;
using SaintsField.Samples.Scripts.SaintsEditor;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class Issue201 : SaintsMonoBehaviour
    {
        [Serializable]
        public struct LOD
        {
            public int distance;
            public Mesh boidMesh;
            public Material boidMaterial;
            // public
        }

        [Table] public LOD[] lods;
    }
}
