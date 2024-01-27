using System;
using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue.Issue1
{
    public class AssetBundleBuildWindow : MonoBehaviour
    {
        [SerializeField, Expandable] private AbConfig _abConfig;

        [SerializeField, Expandable] private AbConfig[] _abConfigs;

        [Serializable]
        public struct ExpandMe
        {
            public int intHere;
        }

        public ExpandMe defaultUnityExpandDraw;
    }
}
