using UnityEngine;

namespace SaintsField.Samples.Scripts.IssueAndTesting.Issue
{
    public class DictionaryOtherTypes : SaintsMonoBehaviour
    {
        public AnimationCurve rac;
        public SaintsDictionary<int, AnimationCurve> ac;
        public SaintsDictionary<int, AnimationCurve[]> acArr;
        public Gradient rg;
        public SaintsDictionary<int, Gradient> g;
        public SaintsDictionary<int, Gradient[]> gArr;
        public RectOffset rro;
        public SaintsDictionary<int, RectOffset> ro;
        public SaintsDictionary<int, RectOffset[]> roArr;
    }
}
